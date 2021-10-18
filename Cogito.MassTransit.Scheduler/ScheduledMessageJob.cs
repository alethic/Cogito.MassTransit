using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cogito.Collections;
using Cogito.MassTransit.Scheduling;

using GreenPipes;

using MassTransit;
using MassTransit.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Quartz;

namespace Cogito.MassTransit.Scheduler
{

    public class ScheduledMessageJob : IJob
    {

        /// <summary>
        /// Used to store job data inside the scheduler.
        /// </summary>
        class ScheduleJobData : SerializedMessage
        {

            public Uri Destination { get; set; }

            public string ExpirationTime { get; set; }

            public string ResponseAddress { get; set; }

            public string FaultAddress { get; set; }

            public string Body { get; set; }

            public string MessageId { get; set; }

            public string MessageType { get; set; }

            public string ContentType { get; set; }

            public string RequestId { get; set; }

            public string CorrelationId { get; set; }

            public string ConversationId { get; set; }

            public string InitiatorId { get; set; }

            public string TokenId { get; set; }

            public string HeadersAsJson { get; set; }

            public string PayloadMessageHeadersAsJson { get; set; }

        }

        class Scheduled
        {

        }

        readonly IBus bus;
        readonly ILogger logger;

        /// <summary>
        /// Initalizes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="logger"></param>
        public ScheduledMessageJob(IBus bus, ILogger logger)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs the job.
        /// </summary>
        /// <param name="context"></param>
        public async Task Execute(IJobExecutionContext context)
        {
            var messageId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var headers = new Dictionary<string, object>()
            {
                ["Scheduler-FireTimeUtc"] = context.FireTimeUtc,
                ["Scheduler-NextFireTimeUtc"] = context.NextFireTimeUtc,
                ["Scheduler-PreviousFireTimeUtc"] = context.PreviousFireTimeUtc,
                ["Scheduler-ScheduledFireTimeUtc"] = context.ScheduledFireTimeUtc,
                ["Schedule"] = new Schedule()
                {
                    GroupName = (string)context.MergedJobDataMap.GetOrDefault("Schedule-GroupName"),
                    Name = (string)context.MergedJobDataMap.GetOrDefault("Schedule-Name"),
                    Interval = GetTimeSpan("Schedule-Interval"),
                    From = GetDateTimeOffset("Schedule-From"),
                    Thru = GetDateTimeOffset("Schedule-Thru"),
                    MisfirePolicy = GetEnum<ScheduleMisfirePolicy>("Schedule-MisfirePolicy") ?? ScheduleMisfirePolicy.Default,
                    RepeatCount = GetInt32("Schedule-RepeatCount") ?? 0,
                }
            };

            int? GetInt32(string key)
            {
                var t = (string)context.MergedJobDataMap.GetOrDefault(key);
                return !string.IsNullOrWhiteSpace(t) ? (int?)int.Parse(t) : null;
            }

            TimeSpan? GetTimeSpan(string key)
            {
                var t = (string)context.MergedJobDataMap.GetOrDefault(key);
                return !string.IsNullOrWhiteSpace(t) ? (TimeSpan?)TimeSpan.Parse(t) : null;
            }

            DateTimeOffset? GetDateTimeOffset(string key)
            {
                var t = (string)context.MergedJobDataMap.GetOrDefault(key);
                return !string.IsNullOrWhiteSpace(t) ? (DateTimeOffset?)DateTimeOffset.Parse(t) : null;
            }

            T? GetEnum<T>(string key)
                where T : struct, IConvertible
            {
                var t = (string)context.MergedJobDataMap.GetOrDefault(key);
                return t != null ? (T?)Enum.Parse(typeof(T), t) : null;
            }

            // retrieve message data
            var data = LoadJobData(context.MergedJobDataMap, messageId, conversationId, headers);
            if (data == null)
                throw new JobExecutionException("Unable to load job data.");

            try
            {
                var sendPipe = CreateMessageContext(data, bus.Address, context.Trigger.Key.Name, headers);
                var endpoint = await bus.GetSendEndpoint(data.Destination);
                await endpoint.Send(new Scheduled(), sendPipe);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred sending message {MessageType} to {Destination}.", data.MessageType, data.Destination);
                throw new JobExecutionException(ex, context.RefireCount < 5);
            }
        }

        /// <summary>
        /// Creates a message context which sends the raw message.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sourceAddress"></param>
        /// <param name="triggerKey"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        IPipe<SendContext> CreateMessageContext(ScheduleJobData data, Uri sourceAddress, string triggerKey, Dictionary<string, object> headers)
        {
            var sendPipe = Pipe.New<SendContext>(x =>
            {
                x.UseExecute(context =>
                {
                    // set scheduling token headers
                    if (Guid.TryParse(data.TokenId, out Guid result))
                        context.Headers.Set(MessageHeaders.SchedulingTokenId, result.ToString("N"));

                    // set quartz trigger header
                    context.Headers.Set(MessageHeaders.QuartzTriggerKey, triggerKey);

                    // set scheduler headers
                    foreach (var kvp in headers)
                        context.Headers.Set(kvp.Key, kvp.Value);
                });
            });

            return new SerializedMessageContextAdapter(sendPipe, data, sourceAddress);
        }

        /// <summary>
        /// Returns a serializable object for the message.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        ScheduleJobData LoadJobData(JobDataMap data, Guid messageId, Guid conversationId, Dictionary<string, object> headers)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var headersJ = data.GetOrDefault("Message-Headers") != null ? JObject.Parse((string)data["Message-Headers"]) : new JObject();
            headersJ.Merge(JObject.FromObject(headers));

            return new ScheduleJobData()
            {
                Destination = new Uri((string)data["Message-Destination"]),
                FaultAddress = (string)data["Message-FaultAddress"],
                ResponseAddress = (string)data["Message-ResponseAddress"],
                ContentType = (string)data["Message-ContentType"],
                MessageId = messageId.ToString(),
                MessageType = (string)JArray.Parse((string)data["Message-MessageType"])[0],
                ConversationId = conversationId.ToString(),
                InitiatorId = (string)data["Message-InitiatorId"],
                ExpirationTime = (string)data["Message-ExpirationTime"],
                HeadersAsJson = headersJ.ToString(),
                PayloadMessageHeadersAsJson = headersJ.ToString(),
                TokenId = (string)data["TokenId"],
                Body = GetBody(data, messageId, conversationId, headers),
            };
        }

        /// <summary>
        /// Attempts to load and hydrate the body.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <param name="conversationId"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        string GetBody(JobDataMap data, Guid messageId, Guid conversationId, Dictionary<string, object> headers)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return
                TryGetJsonBody(data, messageId, conversationId, headers) ??
                TryGetXmlBody(data, messageId, conversationId, headers) ??
                throw new NotSupportedException("Unsupported message type. Only JSON and XML are supported.");
        }

        /// <summary>
        /// Attempts to translate the body as JSON. If the body is of the wrong type, <c>null</c> is returned.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <param name="conversationId"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        string TryGetJsonBody(JobDataMap data, Guid messageId, Guid conversationId, Dictionary<string, object> headers)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (string.Equals((string)data["Message-ContentType"], JsonMessageSerializer.JsonContentType.MediaType, StringComparison.OrdinalIgnoreCase))
                return GetJsonBody(data, messageId, conversationId, headers);
            else
                return null;
        }

        /// <summary>
        /// Attempts to translate the body as XML. If the body is of the wrong type, <c>null</c> is returned.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <param name="conversationId"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        string TryGetXmlBody(JobDataMap data, Guid messageId, Guid conversationId, Dictionary<string, object> headers)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (string.Equals((string)data["Message-ContentType"], XmlMessageSerializer.XmlContentType.MediaType, StringComparison.OrdinalIgnoreCase))
                return GetXmlBody(data, messageId, conversationId, headers);
            else
                return null;
        }

        /// <summary>
        /// Rewrites a JSON message into the designed destination type.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <param name="conversationId"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        string GetJsonBody(JobDataMap data, Guid messageId, Guid conversationId, Dictionary<string, object> headers)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new JObject(
                    new JProperty("messageId", messageId),
                    new JProperty("conversationId", conversationId),
                    new JProperty("sourceAddress", data["Message-SourceAddress"]),
                    new JProperty("faultAddress", data["Message-FaultAddress"]),
                    new JProperty("responseAddress", data["Message-ResponseAddress"]),
                    new JProperty("destinationAddress", data["Message-Destination"]),
                    new JProperty("messageType", JArray.Parse((string)data["Message-MessageType"])),
                    new JProperty("message", JObject.Parse((string)data["Message"])),
                    new JProperty("headers", JObject.FromObject(headers)))
                .ToString(Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Rewrites a XML message into the desired destination type.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <param name="conversationId"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        string GetXmlBody(JobDataMap data, Guid messageId, Guid conversationId, Dictionary<string, object> headers)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            throw new NotImplementedException();
        }

    }

}
