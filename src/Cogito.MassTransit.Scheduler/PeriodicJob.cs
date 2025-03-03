using System;
using System.Reflection;
using System.Threading.Tasks;

using Cogito.DependencyInjection;
using Cogito.MassTransit.Scheduling.Periodic;

using MassTransit;

using Quartz;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Handles raising the appropriate <see cref="P"/> messages.
    /// </summary>
    [AddScopedService<PeriodicJob>()]
    public class PeriodicJob : IJob
    {

        static readonly MethodInfo CreatePeriodicMessageMethodInfo = typeof(PeriodicJob)
            .GetMethod(nameof(CreatePeriodicMessage), BindingFlags.Static | BindingFlags.NonPublic);

        readonly IBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        public PeriodicJob(IBus bus)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <param name="context"></param>
        public async Task Execute(IJobExecutionContext context)
        {
            var message = MakePulse(context);
            if (message == null)
                throw new InvalidOperationException();

            // publish periodic message
            await bus.Publish(message, message.GetType(), ctx =>
            {
                // message is transient and should expire at interval length
                ctx.Durable = false;
                ctx.TimeToLive = TimeSpan.Parse((string)context.MergedJobDataMap["Interval"]);
            });
        }

        /// <summary>
        /// Makes a new <see cref="P"/> object based on the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        P MakePulse(IJobExecutionContext context)
        {
            var intv = TimeSpan.Parse((string)context.MergedJobDataMap["Interval"]);
            var type = CreatePeriodicMessageType(intv);
            var pmsg = CreatePeriodicMessageMethodInfo.MakeGenericMethod(type).Invoke(null, new[] { context });
            return (P)pmsg;
        }

        /// <summary>
        /// Gets the type of pulse to raise given the interval.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        static Type CreatePeriodicMessageType(TimeSpan interval)
        {
            switch (interval)
            {
                case TimeSpan t when t == TimeSpan.FromMinutes(1):
                    return typeof(PT1M);
                case TimeSpan t when t == TimeSpan.FromMinutes(5):
                    return typeof(PT5M);
                case TimeSpan t when t == TimeSpan.FromMinutes(15):
                    return typeof(PT15M);
                case TimeSpan t when t == TimeSpan.FromMinutes(30):
                    return typeof(PT30M);
                case TimeSpan t when t == TimeSpan.FromHours(1):
                    return typeof(PT1H);
                case TimeSpan t when t == TimeSpan.FromHours(2):
                    return typeof(PT2H);
                case TimeSpan t when t == TimeSpan.FromHours(6):
                    return typeof(PT6H);
                case TimeSpan t when t == TimeSpan.FromHours(12):
                    return typeof(PT12H);
                case TimeSpan t when t == TimeSpan.FromDays(1):
                    return typeof(P1D);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Creates a <see cref="P"/> of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        static T CreatePeriodicMessage<T>(IJobExecutionContext context)
            where T : P, new()
        {
            return new T()
            {
                FireTimeUtc = context.FireTimeUtc,
                NextFireTimeUtc = context.NextFireTimeUtc,
                PreviousFireTimeUtc = context.PreviousFireTimeUtc,
                ScheduledFireTimeUtc = context.ScheduledFireTimeUtc,
                Interval = TimeSpan.Parse((string)context.MergedJobDataMap["Interval"])
            };
        }

    }

}
