using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Scheduling
{

    /// <summary>
    /// Describes a scheduled messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScheduledMessage<T> :
        ScheduledMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ScheduledMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="delete"></param>
        public ScheduledMessage(Uri destination, Schedule schedule) :
            base(destination, schedule)
        {

        }

    }

    /// <summary>
    /// Describes a scheduled message.
    /// </summary>
    public class ScheduledMessage
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ScheduledMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="delete"></param>
        public ScheduledMessage(Uri destination, Schedule schedule)
        {
            Destination = destination ?? throw new ArgumentNullException(nameof(schedule));
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        }

        /// <summary>
        /// Destination the message is to be sent to.
        /// </summary>
        public Uri Destination { get; set; }

        /// <summary>
        /// Schedule upon which to deliver message.
        /// </summary>
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Submits a request to delete the scheduled message.
        /// </summary>
        /// <returns></returns>
        public async Task Delete(ConsumeContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            await context.DeleteSchedule(Schedule.GroupName, Schedule.Name);
        }

    }

}
