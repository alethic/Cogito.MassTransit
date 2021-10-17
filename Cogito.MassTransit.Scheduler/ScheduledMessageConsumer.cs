using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Cogito.MassTransit.Scheduling;

using MassTransit;
using MassTransit.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Quartz;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Consumes requests to create or alter scheduled messages.
    /// </summary>
    public class ScheduledMessageConsumer : IConsumer<ScheduleMessage>, IConsumer<DeleteSchedule>
    {

        readonly IScheduler scheduler;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="scheduler"></param>
        public ScheduledMessageConsumer(IScheduler scheduler, ILogger logger)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consumes a request to update or create a scheduled message.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ScheduleMessage> context)
        {
            if (context.Message.Schedule == null)
                throw new NullReferenceException("No schedule specified.");

            // group of schedules
            var keyGroupName = context.Message.Schedule.GroupName ?? "Default";

            // identifier of the schedule
            var keyName = context.Message.Schedule.Name ?? context.Message.CorrelationId?.ToString();
            if (keyName == null)
                throw new InvalidOperationException("No key name could be determined.");

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("ScheduleMessage: [GroupName='{keyGroupName}', Name='{keyName}']", keyGroupName, keyName);

            // create potential new job
            var jobKey = new JobKey(keyName, keyGroupName);
            var job = CreateJobDetail(
                context,
                context.Message.Destination,
                jobKey);

            // create potential new trigger
            var triggerKey = new TriggerKey(keyName, keyGroupName);
            var trigger = CreateTrigger(
                triggerKey,
                jobKey,
                context.Message.Schedule);

            // new trigger set
            var triggers = new HashSet<ITrigger>();
            triggers.Add(trigger);

            // trigger already exists, check for equality; if false, replace
            if (await scheduler.CheckExists(triggerKey))
            {
                var oldTrigger = await scheduler.GetTrigger(triggerKey);
                if (Equals(trigger, oldTrigger) == false)
                {
                    logger.LogInformation("Rescheduling existing job {JobKey} with new triggers.", job.Key, trigger);
                    await scheduler.ScheduleJob(job, triggers, true);
                    return;
                }
            }

            // job already exists, check for equality; if false, replace
            if (await scheduler.CheckExists(jobKey))
            {
                var oldJob = await scheduler.GetJobDetail(jobKey);
                if (Equals(job, oldJob) == false)
                {
                    logger.LogInformation("Rescheduling existing job {JobKey}.", job.Key);
                    await scheduler.ScheduleJob(job, triggers, true);
                    return;
                }
                else
                    logger.LogDebug("No changes detected on existing scheduled job {JobKey}.", job.Key);
            }
            else
            {
                logger.LogInformation("Scheduling new job {JobKey}.", job.Key);
                await scheduler.ScheduleJob(job, triggers, true);
                return;
            }
        }

        /// <summary>
        /// Generates a Quartz trigger from the given trigger.
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <param name="jobKey"></param>
        /// <param name="schedule"></param>
        /// <returns></returns>
        ITrigger CreateTrigger(TriggerKey triggerKey, JobKey jobKey, Schedule schedule)
        {
            if (triggerKey == null)
                throw new ArgumentNullException(nameof(triggerKey));
            if (jobKey == null)
                throw new ArgumentNullException(nameof(jobKey));
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            var b = TriggerBuilder.Create()
                .ForJob(jobKey)
                .WithIdentity(triggerKey)
                .WithSimpleSchedule(i =>
                {
                    ApplyMisfireHandlingInstruction(i, schedule.MisfirePolicy);

                    if (schedule.Interval != null)
                        i.WithInterval((TimeSpan)schedule.Interval);

                    if (schedule.RepeatCount == -1)
                        i.RepeatForever();
                    else
                        i.WithRepeatCount(schedule.RepeatCount);
                });

            if (schedule.From != null)
                b = b.StartAt((DateTimeOffset)schedule.From);

            if (schedule.Thru != null)
                b = b.EndAt((DateTimeOffset)schedule.Thru);

            // apply job data for equality testing
            b = b.UsingJobData("Trigger-MisfirePolicy", schedule.MisfirePolicy.ToString());
            b = b.UsingJobData("Trigger-Interval", schedule.Interval?.ToString() ?? "");
            b = b.UsingJobData("Trigger-RepeatCount", schedule.RepeatCount.ToString());
            b = b.UsingJobData("Trigger-From", schedule.From?.ToString() ?? "");
            b = b.UsingJobData("Trigger-Thru", schedule.Thru?.ToString() ?? "");

            return b.Build();
        }

        /// <summary>
        /// Applies the appropriate misfire handling policy.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="instruction"></param>
        void ApplyMisfireHandlingInstruction(SimpleScheduleBuilder builder, ScheduleMisfirePolicy instruction)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            switch (instruction)
            {
                case ScheduleMisfirePolicy.Default:
                    // apply no policy, use whatever is set by default
                    break;
                case ScheduleMisfirePolicy.FireNow:
                    builder.WithMisfireHandlingInstructionFireNow();
                    break;
                case ScheduleMisfirePolicy.Ignore:
                    builder.WithMisfireHandlingInstructionIgnoreMisfires();
                    break;
                case ScheduleMisfirePolicy.NextWithExistingCount:
                    builder.WithMisfireHandlingInstructionNextWithExistingCount();
                    break;
                case ScheduleMisfirePolicy.NextWithRemainingCount:
                    builder.WithMisfireHandlingInstructionNextWithRemainingCount();
                    break;
                case ScheduleMisfirePolicy.NowWithExistingRepeatCount:
                    builder.WithMisfireHandlingInstructionNowWithExistingCount();
                    break;
                case ScheduleMisfirePolicy.NowWithRemainingRepeatCount:
                    builder.WithMisfireHandlingInstructionNowWithRemainingCount();
                    break;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the specified job is equal to the given job.
        /// </summary>
        /// <param name="job1"></param>
        /// <param name="job2"></param>
        /// <returns></returns>
        bool Equals(IJobDetail job1, IJobDetail job2)
        {
            // reference equality
            if (job1 == job2)
                return true;

            if (job1.Description != job2.Description)
                return false;

            // check that job data is equal
            if (job1.JobDataMap.Keys.Count != job2.JobDataMap.Keys.Count ||
                job1.JobDataMap.Keys.All(k => job2.JobDataMap.ContainsKey(k) && Equals(job1.JobDataMap[k], job2.JobDataMap[k])) == false ||
                job2.JobDataMap.Keys.All(k => job1.JobDataMap.ContainsKey(k) && Equals(job2.JobDataMap[k], job1.JobDataMap[k])) == false)
                return false;

            // must be equal
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the specified trigger is equal to the given trigger.
        /// </summary>
        /// <param name="trigger1"></param>
        /// <param name="trigger2"></param>
        /// <returns></returns>
        bool Equals(ITrigger trigger1, ITrigger trigger2)
        {
            return Equals((ISimpleTrigger)trigger1, (ISimpleTrigger)trigger2);
        }

        /// <summary>
        /// Returns <c>true</c> if the specified trigger is equal to the given trigger.
        /// </summary>
        /// <param name="trigger1"></param>
        /// <param name="trigger2"></param>
        /// <returns></returns>
        bool Equals(ISimpleTrigger trigger1, ISimpleTrigger trigger2)
        {
            // reference equality
            if (trigger1 == trigger2)
                return true;

            if (trigger1.Description != trigger2.Description)
                return false;

            // check that job data is equal
            if (trigger1.JobDataMap.Keys.Count != trigger2.JobDataMap.Keys.Count ||
                trigger1.JobDataMap.Keys.All(k => trigger2.JobDataMap.ContainsKey(k) && Equals(trigger1.JobDataMap[k], trigger2.JobDataMap[k])) == false ||
                trigger2.JobDataMap.Keys.All(k => trigger1.JobDataMap.ContainsKey(k) && Equals(trigger2.JobDataMap[k], trigger1.JobDataMap[k])) == false)
                return false;

            // must be equal
            return true;
        }

        /// <summary>
        /// Creates new job details from the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destination"></param>
        /// <param name="jobKey"></param>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        static IJobDetail CreateJobDetail(ConsumeContext<ScheduleMessage> context, Uri destination, JobKey jobKey, Guid? tokenId = default(Guid?))
        {
            // get the message body
            var data = GetMessageData(context, GetBody(context));
            if (data == null)
                throw new FormatException("Cannot handle given message body type.");

            // create new scheduled job
            return JobBuilder.Create<ScheduledMessageJob>()
                .WithIdentity(jobKey)
                .RequestRecovery(true)
                .UsingJobData("Message-Destination", ToString(destination))
                .UsingJobData("Message-ResponseAddress", ToString(context.ResponseAddress))
                .UsingJobData("Message-FaultAddress", ToString(context.FaultAddress))
                .UsingJobData("Message-SourceAddress", ToString(context.SourceAddress))
                .UsingJobData("Message-MessageType", JsonConvert.SerializeObject(data.MessageType))
                .UsingJobData("Message-ContentType", context.ReceiveContext.ContentType.MediaType)
                .UsingJobData("Message-ExpirationTime", context.ExpirationTime?.ToString() ?? "")
                .UsingJobData("Message-Headers", JsonConvert.SerializeObject(context.Headers.GetAll()))
                .UsingJobData("Message", data.Message ?? "")
                .UsingJobData("Schedule-GroupName", context.Message.Schedule.GroupName ?? "")
                .UsingJobData("Schedule-Name", context.Message.Schedule.Name ?? "")
                .UsingJobData("Schedule-Interval", context.Message.Schedule.Interval?.ToString() ?? "")
                .UsingJobData("Schedule-From", context.Message.Schedule.From?.ToString() ?? "")
                .UsingJobData("Schedule-Thru", context.Message.Schedule.Thru?.ToString() ?? "")
                .UsingJobData("Schedule-MisfirePolicy", context.Message.Schedule.MisfirePolicy.ToString())
                .UsingJobData("Schedule-RepeatCount", context.Message.Schedule.RepeatCount.ToString())
                .UsingJobData("TokenId", tokenId?.ToString("N") ?? "")
                .Build();
        }

        /// <summary>
        /// Extracts the message body from the context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static string GetBody(ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            using (var bodyStream = context.ReceiveContext.GetBodyStream())
            using (var rdr = new StreamReader(bodyStream))
                return rdr.ReadToEnd();
        }

        /// <summary>
        /// Transforms the body from it's native type to what will be scheduled to be sent.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        static ScheduledMessageJobData GetMessageData(ConsumeContext context, string body)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            return TryGetJsonMessageData(context, body) ?? TryGetXmlMessageData(context, body);
        }

        /// <summary>
        /// Attempts to translate the body as JSON. If the body is of the wrong type, <c>null</c> is returned.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        static ScheduledMessageJobData TryGetJsonMessageData(ConsumeContext context, string body)
        {
            if (string.Equals(context.ReceiveContext.ContentType.MediaType, JsonMessageSerializer.JsonContentType.MediaType, StringComparison.OrdinalIgnoreCase))
                return GetJsonMessageData(body);
            else
                return null;
        }

        /// <summary>
        /// Attempts to translate the body as XML. If the body is of the wrong type, <c>null</c> is returned.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        static ScheduledMessageJobData TryGetXmlMessageData(ConsumeContext context, string body)
        {
            if (string.Equals(context.ReceiveContext.ContentType.MediaType, XmlMessageSerializer.XmlContentType.MediaType, StringComparison.OrdinalIgnoreCase))
                return GetXmlMessageData(body);
            else
                return null;
        }

        /// <summary>
        /// Returns the string version of a URI or an empty string.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        static string ToString(Uri uri)
        {
            return uri?.ToString() ?? "";
        }

        /// <summary>
        /// Rewrites a JSON message into the designed destination type.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        static ScheduledMessageJobData GetJsonMessageData(string body)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var envelope = JObject.Parse(body);
            var message = envelope["message"];
            var payload = message["payload"];
            var payloadType = message["payloadType"];

            return new ScheduledMessageJobData()
            {
                MessageType = payloadType.ToObject<string[]>(),
                Message = payload.ToString(Formatting.Indented),
            };
        }

        /// <summary>
        /// Rewrites a XML message into the desired destination type.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        static ScheduledMessageJobData GetXmlMessageData(string body)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            using (var reader = new StringReader(body))
            {
                // parse message document
                var document = XDocument.Load(reader);
                var envelope = document.Descendants("envelope").Single();
                var message = envelope.Descendants("message").Single();
                var payload = message.Descendants("payload").Single();
                var payloadType = message.Descendants("payloadType");

                return new ScheduledMessageJobData()
                {
                    MessageType = payloadType.Select(i => i.Value).ToArray(),
                    Message = new XElement("message", payload.Elements()).ToString(),
                };
            }
        }

        public Task Consume(ConsumeContext<DeleteSchedule> context)
        {
            // find unique values
            var keyGroupName = context.Message.GroupName;
            var keyName = context.Message.Name;

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("DeleteSchedule: [GroupName='{keyGroupName}', Name='{keyName}']", keyGroupName, keyName);

            // delete existing job
            var jobKey = new JobKey(keyName, keyGroupName);
            var job = scheduler.GetJobDetail(jobKey);
            if (job != null)
                scheduler.DeleteJob(jobKey);

            return Task.FromResult(true);
        }

    }

}