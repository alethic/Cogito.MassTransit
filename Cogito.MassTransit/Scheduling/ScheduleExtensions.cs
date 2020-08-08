using System;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;
using MassTransit.Metadata;

namespace Cogito.MassTransit.Scheduling
{

    /// <summary>
    /// Various extensions to schedule messages.
    /// </summary>
    public static class ScheduleExtensions
    {

        /// <summary>
        /// Schedules the given message to be published on the given schedule.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bus"></param>
        /// <param name="schedule"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ScheduledMessage<T>> ScheduleMessage<T>(
            this IBus bus,
            Schedule schedule,
            T message,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            return await ScheduleMessage<T>(
                bus,
                bus.Address,
                schedule,
                message,
                cancellationToken);
        }

        /// <summary>
        /// Schedules the given message to be published on the given schedule.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bus"></param>
        /// <param name="destination"></param>
        /// <param name="schedule"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ScheduledMessage<T>> ScheduleMessage<T>(
            this IBus bus,
            Uri destination,
            Schedule schedule,
            T message,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // endpoint of scheduler
            var i = await bus.GetSendEndpoint(new Uri(bus.Address, "/scheduler"));
            if (i == null)
                throw new InvalidOperationException("Cannot find send endpoint.");

            // command to schedule the message
            var s = new ScheduleMessage<T>()
            {
                Destination = destination,
                Schedule = schedule,
                Payload = message,
                PayloadType = TypeMetadataCache<T>.MessageTypeNames,
            };

            // send to endpoint to schedule
            await i.Send(s, cancellationToken);

            // handler to potentially cancel the message
            return new ScheduledMessage<T>(s.Destination, s.Schedule);
        }

        /// <summary>
        /// Schedules the given message to be published on the given schedule.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="schedule"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ScheduledMessage<T>> ScheduleMessage<T>(
            this ConsumeContext context,
            Schedule schedule,
            T message,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            return await ScheduleMessage<T>(
                context,
                context.ReceiveContext.InputAddress,
                schedule,
                message,
                cancellationToken);
        }

        /// <summary>
        /// Schedules the given message to be published on the given schedule.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="destination"></param>
        /// <param name="schedule"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ScheduledMessage<T>> ScheduleMessage<T>(
            this ConsumeContext context,
            Uri destination,
            Schedule schedule,
            T message,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // endpoint of scheduler
            var i = await context.GetSendEndpoint(new Uri(context.ReceiveContext.InputAddress, "/scheduler"));
            if (i == null)
                throw new InvalidOperationException("Cannot find send endpoint.");

            // command to schedule the message
            var s = new ScheduleMessage<T>()
            {
                Destination = destination,
                Schedule = schedule,
                Payload = message,
                PayloadType = TypeMetadataCache<T>.MessageTypeNames,
            };

            // send to endpoint to schedule
            await i.Send(s, cancellationToken);

            // handler to potentially cancel the message
            return new ScheduledMessage<T>(s.Destination, s.Schedule);
        }

        /// <summary>
        /// Deletes the scheduled message with the specified group and name.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="groupName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task DeleteSchedule(
            this ConsumeContext context,
            string groupName,
            string name)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (groupName == null)
                throw new ArgumentNullException(nameof(groupName));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            // endpoint of scheduler
            var i = await context.GetSendEndpoint(new Uri(context.ReceiveContext.InputAddress, "/scheduler"));
            if (i == null)
                throw new InvalidOperationException("Could not resolve scheduler endpoint.");

            await i.Send(new DeleteSchedule()
            {
                GroupName = groupName,
                Name = name,
            });
        }

        /// <summary>
        /// Deletes the scheduled message currently being delivered.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task DeleteSchedule(this ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var s = GetSchedule(context);
            if (s != null)
                await DeleteSchedule(context, s.GroupName, s.Name);
        }

        /// <summary>
        /// Returns the scheduling information of the currently received message, if any is present.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Schedule GetSchedule(this ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Headers.Get<Schedule>("Schedule", null);
        }

        /// <summary>
        /// Gets the scheduled fire time.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTimeOffset? GetFireTime(this ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Headers.Get<DateTimeOffset>("Scheduler-FireTimeUtc", null);
        }

        /// <summary>
        /// Gets the scheduled fire time.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTimeOffset? GetNextFireTime(this ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Headers.Get<DateTimeOffset>("Scheduler-NextFireTimeUtc", null);
        }

        /// <summary>
        /// Gets the scheduled fire time.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTimeOffset? GetPreviousFireTime(this ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Headers.Get<DateTimeOffset>("Scheduler-PreviousFireTimeUtc", null);
        }

        /// <summary>
        /// Gets the scheduled fire time.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTimeOffset? GetScheduledFireTime(this ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Headers.Get<DateTimeOffset>("Scheduler-ScheduledFireTimeUtc", null);
        }

    }

}
