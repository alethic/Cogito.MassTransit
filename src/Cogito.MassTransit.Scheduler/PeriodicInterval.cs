using System;
using System.Collections.Generic;
using System.Linq;

using Cogito.MassTransit.Scheduling.Periodic;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Describes one of the fixed periods on which a periodic message is published. Every period divides a day
    /// evenly, and is aligned to midnight UTC.
    /// </summary>
    public sealed class PeriodicInterval
    {

        /// <summary>
        /// Every minute.
        /// </summary>
        public static readonly PeriodicInterval PT1M = Create<PT1M>(nameof(PT1M), TimeSpan.FromMinutes(1));

        /// <summary>
        /// Every five minutes.
        /// </summary>
        public static readonly PeriodicInterval PT5M = Create<PT5M>(nameof(PT5M), TimeSpan.FromMinutes(5));

        /// <summary>
        /// Every fifteen minutes.
        /// </summary>
        public static readonly PeriodicInterval PT15M = Create<PT15M>(nameof(PT15M), TimeSpan.FromMinutes(15));

        /// <summary>
        /// Every thirty minutes.
        /// </summary>
        public static readonly PeriodicInterval PT30M = Create<PT30M>(nameof(PT30M), TimeSpan.FromMinutes(30));

        /// <summary>
        /// Every hour.
        /// </summary>
        public static readonly PeriodicInterval PT1H = Create<PT1H>(nameof(PT1H), TimeSpan.FromHours(1));

        /// <summary>
        /// Every two hours.
        /// </summary>
        public static readonly PeriodicInterval PT2H = Create<PT2H>(nameof(PT2H), TimeSpan.FromHours(2));

        /// <summary>
        /// Every six hours.
        /// </summary>
        public static readonly PeriodicInterval PT6H = Create<PT6H>(nameof(PT6H), TimeSpan.FromHours(6));

        /// <summary>
        /// Every twelve hours.
        /// </summary>
        public static readonly PeriodicInterval PT12H = Create<PT12H>(nameof(PT12H), TimeSpan.FromHours(12));

        /// <summary>
        /// Every day.
        /// </summary>
        public static readonly PeriodicInterval P1D = Create<P1D>(nameof(P1D), TimeSpan.FromDays(1));

        /// <summary>
        /// Gets every periodic interval, shortest first.
        /// </summary>
        public static IReadOnlyList<PeriodicInterval> All { get; } = new[] { PT1M, PT5M, PT15M, PT30M, PT1H, PT2H, PT6H, PT12H, P1D };

        /// <summary>
        /// Gets the periodic interval with the specified name, such as <c>PT5M</c>, or <c>null</c> if there is none.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PeriodicInterval? FromName(string name)
        {
            return All.FirstOrDefault(i => i.Name == name);
        }

        /// <summary>
        /// Gets the periodic interval of the specified length, or <c>null</c> if there is none.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static PeriodicInterval? FromInterval(TimeSpan interval)
        {
            return All.FirstOrDefault(i => i.Interval == interval);
        }

        static PeriodicInterval Create<T>(string name, TimeSpan interval)
            where T : P, new()
        {
            return new PeriodicInterval(name, interval, typeof(T), () => new T());
        }

        readonly Func<P> factory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        /// <param name="messageType"></param>
        /// <param name="factory"></param>
        PeriodicInterval(string name, TimeSpan interval, Type messageType, Func<P> factory)
        {
            Name = name;
            Interval = interval;
            MessageType = messageType;
            this.factory = factory;
        }

        /// <summary>
        /// Gets the ISO 8601 name of the interval, which is also the name of its message type.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the length of the interval.
        /// </summary>
        public TimeSpan Interval { get; }

        /// <summary>
        /// Gets the type of message published on the interval.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// Gets the time at which the interval most recently began, at or before <paramref name="time"/>.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public DateTimeOffset GetScheduledFireTime(DateTimeOffset time)
        {
            var utc = time.ToUniversalTime();
            var midnight = new DateTimeOffset(utc.Year, utc.Month, utc.Day, 0, 0, 0, TimeSpan.Zero);
            return midnight + TimeSpan.FromTicks((utc - midnight).Ticks / Interval.Ticks * Interval.Ticks);
        }

        /// <summary>
        /// Creates the message published on the interval.
        /// </summary>
        /// <param name="fireTimeUtc">The time at which the schedule actually fired.</param>
        /// <param name="scheduledFireTimeUtc">The time at which the schedule was due to fire.</param>
        /// <param name="previousFireTimeUtc">The time at which the schedule previously fired.</param>
        /// <param name="nextFireTimeUtc">The time at which the schedule next fires.</param>
        /// <returns></returns>
        public P CreateMessage(DateTimeOffset? fireTimeUtc, DateTimeOffset? scheduledFireTimeUtc, DateTimeOffset? previousFireTimeUtc, DateTimeOffset? nextFireTimeUtc)
        {
            var message = factory();
            message.Interval = Interval;
            message.FireTimeUtc = fireTimeUtc;
            message.ScheduledFireTimeUtc = scheduledFireTimeUtc;
            message.PreviousFireTimeUtc = previousFireTimeUtc;
            message.NextFireTimeUtc = nextFireTimeUtc;
            return message;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

    }

}
