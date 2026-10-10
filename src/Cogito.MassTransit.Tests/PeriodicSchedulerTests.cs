using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cogito.MassTransit.Scheduler;
using Cogito.MassTransit.Scheduling.Periodic;

using Hangfire;
using Hangfire.Storage;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Quartz;
using Quartz.Impl.Matchers;

using Xunit;

using HangfirePeriodicJob = Cogito.MassTransit.Scheduler.HangfireIntegration.PeriodicJob;
using HangfirePeriodicJobRegistrar = Cogito.MassTransit.Scheduler.HangfireIntegration.PeriodicJobRegistrar;
using QuartzPeriodicJob = Cogito.MassTransit.Scheduler.QuartzIntegration.PeriodicJob;
using QuartzPeriodicScheduler = Cogito.MassTransit.Scheduler.QuartzIntegration.PeriodicScheduler;

namespace Cogito.MassTransit.Tests
{

    public class PeriodicIntervalTests
    {

        [Fact]
        public void Every_interval_is_named_for_its_message_type_and_can_be_found()
        {
            Assert.Equal(new[] { "PT1M", "PT5M", "PT15M", "PT30M", "PT1H", "PT2H", "PT6H", "PT12H", "P1D" }, PeriodicInterval.All.Select(i => i.Name));

            foreach (var interval in PeriodicInterval.All)
            {
                Assert.Equal(interval.Name, interval.MessageType.Name);
                Assert.Same(interval, PeriodicInterval.FromName(interval.Name));
                Assert.Same(interval, PeriodicInterval.FromInterval(interval.Interval));
            }

            Assert.Null(PeriodicInterval.FromName("PT7M"));
            Assert.Null(PeriodicInterval.FromInterval(TimeSpan.FromMinutes(7)));
        }

        [Theory]
        [InlineData("PT1M", "2026-10-10T13:07:42Z", "2026-10-10T13:07:00Z")]
        [InlineData("PT5M", "2026-10-10T13:07:42Z", "2026-10-10T13:05:00Z")]
        [InlineData("PT5M", "2026-10-10T13:05:00Z", "2026-10-10T13:05:00Z")]
        [InlineData("PT2H", "2026-10-10T13:07:42Z", "2026-10-10T12:00:00Z")]
        [InlineData("PT12H", "2026-10-10T11:59:59Z", "2026-10-10T00:00:00Z")]
        [InlineData("P1D", "2026-10-10T23:59:59Z", "2026-10-10T00:00:00Z")]
        [InlineData("PT6H", "2026-10-10T01:07:00+05:00", "2026-10-09T18:00:00Z")]
        public void Scheduled_fire_time_is_the_start_of_the_interval_aligned_to_midnight_utc(string name, string time, string expected)
        {
            var interval = PeriodicInterval.FromName(name)!;
            Assert.Equal(DateTimeOffset.Parse(expected), interval.GetScheduledFireTime(DateTimeOffset.Parse(time)));
        }

        [Fact]
        public void Creates_the_message_of_the_interval()
        {
            var fire = DateTimeOffset.Parse("2026-10-10T13:05:01Z");
            var scheduled = DateTimeOffset.Parse("2026-10-10T13:05:00Z");

            var message = PeriodicInterval.PT5M.CreateMessage(fire, scheduled, scheduled.AddMinutes(-5), scheduled.AddMinutes(5));

            Assert.IsType<PT5M>(message);
            Assert.Equal(TimeSpan.FromMinutes(5), message.Interval);
            Assert.Equal(fire, message.FireTimeUtc);
            Assert.Equal(scheduled, message.ScheduledFireTimeUtc);
            Assert.Equal(scheduled.AddMinutes(-5), message.PreviousFireTimeUtc);
            Assert.Equal(scheduled.AddMinutes(5), message.NextFireTimeUtc);
        }

    }

    public class PeriodicJobSchedulerRegistrationTests
    {

        [Fact]
        public void A_scheduler_must_be_selected()
        {
            var e = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddPeriodicJobScheduler(c => { }));
            Assert.Contains("UseQuartz", e.Message);
        }

        [Fact]
        public void Only_one_scheduler_may_be_selected()
        {
            var e = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddPeriodicJobScheduler(c => c.UseQuartz().UseHangfire()));
            Assert.Contains("Quartz", e.Message);
        }

        [Fact]
        public async Task Publishes_periodic_messages_transient_and_expiring_after_their_interval()
        {
            await using var provider = new ServiceCollection()
                .AddMassTransitTestHarness()
                .AddSingleton<IPeriodicMessagePublisher, PeriodicMessagePublisher>()
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            await provider.GetRequiredService<IPeriodicMessagePublisher>().Publish(PeriodicInterval.PT15M.CreateMessage(null, null, null, null));

            Assert.True(await harness.Published.Any<PT15M>());
            var published = harness.Published.Select<PT15M>().First();
            Assert.False(published.Context.Durable);
            Assert.Equal(TimeSpan.FromMinutes(15), published.Context.TimeToLive);
        }

    }

    public class QuartzPeriodicJobSchedulerTests
    {

        static readonly JobKey PeriodicJobKey = new JobKey("Periodic", "Cogito.MassTransit.Scheduler");

        /// <summary>
        /// A job of another type, standing in for one stored by an earlier version under the same key.
        /// </summary>
        public class OtherJob : IJob
        {

            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;

        }

        static ServiceProvider BuildProvider()
        {
            return new ServiceCollection()
                .AddQuartz()
                .AddMassTransitTestHarness()
                .AddPeriodicJobScheduler(c => c.UseQuartz())
                .BuildServiceProvider(true);
        }

        static async Task<IReadOnlyCollection<ITrigger>> WaitForTriggers(IScheduler scheduler, int count)
        {
            var until = DateTime.UtcNow.AddSeconds(30);

            while (true)
            {
                var triggers = await scheduler.GetTriggersOfJob(PeriodicJobKey);
                if (triggers.Count == count || DateTime.UtcNow > until)
                    return triggers;

                await Task.Delay(100);
            }
        }

        [Fact]
        public async Task Schedules_a_trigger_for_every_interval_and_publishes_when_one_fires()
        {
            await using var provider = BuildProvider();
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var scheduler = await provider.GetRequiredService<ISchedulerFactory>().GetScheduler();
            await scheduler.Start();

            var service = provider.GetServices<IHostedService>().OfType<QuartzPeriodicScheduler>().Single();
            await service.StartAsync(default);

            try
            {
                var triggers = await WaitForTriggers(scheduler, PeriodicInterval.All.Count);
                Assert.Equal(PeriodicInterval.All.Select(i => i.Name).OrderBy(n => n), triggers.Select(t => t.Key.Name).OrderBy(n => n));
                Assert.Equal(typeof(QuartzPeriodicJob), (await scheduler.GetJobDetail(PeriodicJobKey))!.JobType);

                await scheduler.TriggerJob(PeriodicJobKey, new JobDataMap() { [QuartzPeriodicJob.IntervalKey] = TimeSpan.FromMinutes(5).ToString() });

                Assert.True(await harness.Published.Any<PT5M>());
                var message = harness.Published.Select<PT5M>().First().Context.Message;
                Assert.Equal(TimeSpan.FromMinutes(5), message.Interval);
                Assert.NotNull(message.FireTimeUtc);
            }
            finally
            {
                await service.StopAsync(default);
                await scheduler.Shutdown(true);
            }
        }

        [Fact]
        public async Task Replaces_a_stored_job_of_another_type()
        {
            await using var provider = BuildProvider();
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var scheduler = await provider.GetRequiredService<ISchedulerFactory>().GetScheduler();
            await scheduler.AddJob(JobBuilder.Create<OtherJob>().WithIdentity(PeriodicJobKey).UsingJobData("Version", "2").StoreDurably().Build(), false);
            await scheduler.Start();

            var service = provider.GetServices<IHostedService>().OfType<QuartzPeriodicScheduler>().Single();
            await service.StartAsync(default);

            try
            {
                await WaitForTriggers(scheduler, PeriodicInterval.All.Count);
                Assert.Equal(typeof(QuartzPeriodicJob), (await scheduler.GetJobDetail(PeriodicJobKey))!.JobType);
                Assert.Single(await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(PeriodicJobKey.Group)), k => k.Equals(PeriodicJobKey));
            }
            finally
            {
                await service.StopAsync(default);
                await scheduler.Shutdown(true);
            }
        }

    }

    public class HangfirePeriodicJobSchedulerTests
    {

        static ServiceProvider BuildProvider()
        {
            return new ServiceCollection()
                .AddHangfire(c => c.UseInMemoryStorage())
                .AddMassTransitTestHarness()
                .AddPeriodicJobScheduler(c => c.UseHangfire())
                .BuildServiceProvider(true);
        }

        [Theory]
        [InlineData("PT1M", "* * * * *")]
        [InlineData("PT5M", "*/5 * * * *")]
        [InlineData("PT15M", "*/15 * * * *")]
        [InlineData("PT30M", "*/30 * * * *")]
        [InlineData("PT1H", "0 * * * *")]
        [InlineData("PT2H", "0 */2 * * *")]
        [InlineData("PT6H", "0 */6 * * *")]
        [InlineData("PT12H", "0 */12 * * *")]
        [InlineData("P1D", "0 0 * * *")]
        public void Each_interval_fires_on_a_cron_aligned_to_midnight(string name, string cron)
        {
            Assert.Equal(cron, HangfirePeriodicJobRegistrar.GetCronExpression(PeriodicInterval.FromName(name)!));
        }

        [Fact]
        public async Task Adds_a_recurring_job_for_every_interval_in_utc()
        {
            await using var provider = BuildProvider();

            var registrar = provider.GetServices<IHostedService>().OfType<HangfirePeriodicJobRegistrar>().Single();
            await registrar.StartAsync(default);

            // starting again, as another node would, updates rather than duplicates
            await registrar.StartAsync(default);

            using var connection = provider.GetRequiredService<JobStorage>().GetConnection();
            var jobs = connection.GetRecurringJobs();

            Assert.Equal(PeriodicInterval.All.Count, jobs.Count);

            foreach (var interval in PeriodicInterval.All)
            {
                var job = Assert.Single(jobs, j => j.Id == HangfirePeriodicJobRegistrar.GetRecurringJobId(interval));
                Assert.Equal(HangfirePeriodicJobRegistrar.GetCronExpression(interval), job.Cron);
                Assert.Equal(TimeZoneInfo.Utc.Id, job.TimeZoneId);
                Assert.Equal(typeof(HangfirePeriodicJob), job.Job.Type);
                Assert.Equal(interval.Name, job.Job.Args[0]);
            }
        }

        [Fact]
        public async Task Publishes_the_message_for_the_interval_in_which_it_fires()
        {
            await using var provider = BuildProvider();
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var before = DateTimeOffset.UtcNow;
            await ActivatorUtilities.CreateInstance<HangfirePeriodicJob>(provider).Execute("PT5M", null, default);

            Assert.True(await harness.Published.Any<PT5M>());
            var message = harness.Published.Select<PT5M>().First().Context.Message;

            var scheduled = PeriodicInterval.PT5M.GetScheduledFireTime(message.FireTimeUtc!.Value);
            Assert.True(message.FireTimeUtc >= before);
            Assert.Equal(scheduled, message.ScheduledFireTimeUtc);
            Assert.Equal(scheduled.AddMinutes(-5), message.PreviousFireTimeUtc);
            Assert.Equal(scheduled.AddMinutes(5), message.NextFireTimeUtc);
        }

    }

}
