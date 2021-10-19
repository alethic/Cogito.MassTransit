namespace Cogito.MassTransit.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using global::MassTransit;
    using global::MassTransit.Context;
    using global::MassTransit.Scheduling;
    using global::MassTransit.Serialization;
    using global::Quartz;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class NativeScheduleMessageConsumer :
        IConsumer<global::MassTransit.Scheduling.ScheduleMessage>,
        IConsumer<global::MassTransit.Scheduling.ScheduleRecurringMessage>
    {
        readonly Task<global::Quartz.IScheduler> _schedulerTask;
        global::Quartz.IScheduler _scheduler;

        public NativeScheduleMessageConsumer(global::Quartz.IScheduler scheduler)
        {
            _scheduler = scheduler;
            _schedulerTask = Task.FromResult(scheduler);
        }

        public NativeScheduleMessageConsumer(Task<global::Quartz.IScheduler> schedulerTask)
        {
            _schedulerTask = schedulerTask;
        }

        public async Task Consume(ConsumeContext<global::MassTransit.Scheduling.ScheduleMessage> context)
        {
            var correlationId = context.Message.CorrelationId.ToString("N");

            var jobKey = new global::Quartz.JobKey(correlationId);

            var jobDetail = await CreateJobDetail(context, context.Message.Destination, jobKey, context.Message.CorrelationId).ConfigureAwait(false);

            var triggerKey = new global::Quartz.TriggerKey(correlationId);
            var trigger = global::Quartz.TriggerBuilder.Create()
                .ForJob(jobDetail)
                .StartAt(context.Message.ScheduledTime)
                .WithSchedule(global::Quartz.SimpleScheduleBuilder.Create().WithMisfireHandlingInstructionFireNow())
                .WithIdentity(triggerKey)
                .Build();

            var scheduler = _scheduler ??= await _schedulerTask.ConfigureAwait(false);

            if (await scheduler.CheckExists(trigger.Key, context.CancellationToken).ConfigureAwait(false))
                await scheduler.UnscheduleJob(trigger.Key, context.CancellationToken).ConfigureAwait(false);

            await scheduler.ScheduleJob(jobDetail, trigger, context.CancellationToken).ConfigureAwait(false);

            LogContext.Debug?.Log("Scheduled: {Key} {Schedule}", jobKey, trigger.GetNextFireTimeUtc());
        }

        public async Task Consume(ConsumeContext<global::MassTransit.Scheduling.ScheduleRecurringMessage> context)
        {
            var jobKey = new global::Quartz.JobKey(context.Message.Schedule.ScheduleId, context.Message.Schedule.ScheduleGroup);

            var jobDetail = await CreateJobDetail(context, context.Message.Destination, jobKey).ConfigureAwait(false);

            var triggerKey = new global::Quartz.TriggerKey("Recurring.Trigger." + context.Message.Schedule.ScheduleId, context.Message.Schedule.ScheduleGroup);

            var trigger = CreateTrigger(context.Message.Schedule, jobDetail, triggerKey);

            var scheduler = _scheduler ??= await _schedulerTask.ConfigureAwait(false);

            if (await scheduler.CheckExists(triggerKey, context.CancellationToken).ConfigureAwait(false))
                await scheduler.UnscheduleJob(triggerKey, context.CancellationToken).ConfigureAwait(false);

            await scheduler.ScheduleJob(jobDetail, trigger, context.CancellationToken).ConfigureAwait(false);

            LogContext.Debug?.Log("Scheduled: {Key} {Schedule}", jobKey, trigger.GetNextFireTimeUtc());
        }

        global::Quartz.ITrigger CreateTrigger(global::MassTransit.Scheduling.RecurringSchedule schedule, global::Quartz.IJobDetail jobDetail, global::Quartz.TriggerKey triggerKey)
        {
            var tz = TimeZoneInfo.Local;
            if (!string.IsNullOrWhiteSpace(schedule.TimeZoneId) && schedule.TimeZoneId != tz.Id)
                tz = global::Quartz.Util.TimeZoneUtil.FindTimeZoneById(schedule.TimeZoneId);

            var triggerBuilder = global::Quartz.TriggerBuilder.Create()
                .ForJob(jobDetail)
                .WithIdentity(triggerKey)
                .StartAt(schedule.StartTime)
                .WithDescription(schedule.Description)
                .WithCronSchedule(schedule.CronExpression, x =>
                {
                    x.InTimeZone(tz);
                    switch (schedule.MisfirePolicy)
                    {
                        case MissedEventPolicy.Skip:
                            x.WithMisfireHandlingInstructionDoNothing();
                            break;

                        case MissedEventPolicy.Send:
                            x.WithMisfireHandlingInstructionFireAndProceed();
                            break;
                    }
                });

            if (schedule.EndTime.HasValue)
                triggerBuilder.EndAt(schedule.EndTime);

            return triggerBuilder.Build();
        }

        static async Task<global::Quartz.IJobDetail> CreateJobDetail(ConsumeContext context, Uri destination, global::Quartz.JobKey jobKey, Guid? tokenId = default)
        {
            var body = Encoding.UTF8.GetString(context.ReceiveContext.GetBody());

            var mediaType = context.ReceiveContext.ContentType?.MediaType;

            if (JsonMessageSerializer.JsonContentType.MediaType.Equals(mediaType, StringComparison.OrdinalIgnoreCase))
                body = TranslateJsonBody(body, destination.ToString());
            else if (XmlMessageSerializer.XmlContentType.MediaType.Equals(mediaType, StringComparison.OrdinalIgnoreCase))
                body = TranslateXmlBody(body, destination.ToString());
            else
                throw new InvalidOperationException("Only JSON and XML messages can be scheduled");

            var builder = global::Quartz.JobBuilder.Create<ScheduledMessageJob>()
                .RequestRecovery()
                .WithIdentity(jobKey)
                .UsingJobData("Destination", ToString(destination))
                .UsingJobData("ResponseAddress", ToString(context.ResponseAddress))
                .UsingJobData("FaultAddress", ToString(context.FaultAddress))
                .UsingJobData("Body", body)
                .UsingJobData("ContentType", mediaType);

            if (context.MessageId.HasValue)
                builder = builder.UsingJobData("MessageId", context.MessageId.Value.ToString());

            if (context.CorrelationId.HasValue)
                builder = builder.UsingJobData("CorrelationId", context.CorrelationId.Value.ToString());

            if (context.ConversationId.HasValue)
                builder = builder.UsingJobData("ConversationId", context.ConversationId.Value.ToString());

            if (context.InitiatorId.HasValue)
                builder = builder.UsingJobData("InitiatorId", context.InitiatorId.Value.ToString());

            if (context.RequestId.HasValue)
                builder = builder.UsingJobData("RequestId", context.RequestId.Value.ToString());

            if (context.ExpirationTime.HasValue)
                builder = builder.UsingJobData("ExpirationTime", context.ExpirationTime.Value.ToString("O"));

            if (tokenId.HasValue)
                builder = builder.UsingJobData("TokenId", tokenId.Value.ToString("N"));

            IEnumerable<KeyValuePair<string, object>> headers = context.Headers.GetAll();
            if (headers.Any())
                builder = builder.UsingJobData("HeadersAsJson", JsonConvert.SerializeObject(headers));

            var jobDetail = builder
                .Build();

            return jobDetail;
        }

        static string ToString(Uri uri)
        {
            return uri?.ToString() ?? "";
        }

        static string TranslateJsonBody(string body, string destination)
        {
            var envelope = JObject.Parse(body);

            envelope["destinationAddress"] = destination;

            var message = envelope["message"];

            var payload = message["payload"];
            var payloadType = message["payloadType"];

            envelope["message"] = payload;
            envelope["messageType"] = payloadType;

            return JsonConvert.SerializeObject(envelope, Formatting.Indented);
        }

        static string TranslateXmlBody(string body, string destination)
        {
            using var reader = new StringReader(body);

            var document = XDocument.Load(reader);

            var envelope = (from e in document.Descendants("envelope") select e).Single();

            var destinationAddress = (from a in envelope.Descendants("destinationAddress") select a).Single();

            var message = (from m in envelope.Descendants("message") select m).Single();
            IEnumerable<XElement> messageType = from mt in envelope.Descendants("messageType") select mt;

            var payload = (from p in message.Descendants("payload") select p).Single();
            IEnumerable<XElement> payloadType = from pt in message.Descendants("payloadType") select pt;

            message.Remove();
            messageType.Remove();

            destinationAddress.Value = destination;

            message = new XElement("message");
            message.Add(payload.Descendants());
            envelope.Add(message);

            envelope.Add(payloadType.Select(x => new XElement("messageType", x.Value)));

            return document.ToString(SaveOptions.DisableFormatting);
        }
    }
}
