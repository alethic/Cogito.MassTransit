using System;
using System.Collections.Generic;
using System.Linq;

using Quartz;

namespace Cogito.MassTransit.Scheduler.Util
{

    public class TriggerEqualityComparer :
        IEqualityComparer<ITrigger>
    {

        public bool Equals(ITrigger x, ITrigger y)
        {
            if (x.GetType() != y.GetType())
                return false;

            switch (x)
            {
                case ISimpleTrigger t:
                    return TriggerEquals(t, (ISimpleTrigger)y);
                case ICalendarIntervalTrigger t:
                    return TriggerEquals(t, (ICalendarIntervalTrigger)y);
                case IDailyTimeIntervalTrigger t:
                    return TriggerEquals(t, (IDailyTimeIntervalTrigger)y);
                case ICronTrigger t:
                    return TriggerEquals(t, (ICronTrigger)y);
                default:
                    return false;
            }
        }

        bool Equals<T>(T x, T y, Func<T, object> func)
        {
            return object.Equals(func(x), func(y));
        }

        bool Equals<T>(T x, T y, IEnumerable<Func<T, object>> funcs)
        {
            return funcs.All(i => Equals(x, y, i));
        }

        bool Equals<T>(T x, T y, params Func<T, object>[] funcs)
        {
            return Equals(x, y, funcs.AsEnumerable());
        }

        bool TriggerEquals(ITrigger x, ITrigger y)
        {
            return Equals(x, y,
                i => i.CalendarName,
                i => i.Description,
                i => i.EndTimeUtc,
                i => i.JobKey,
                i => i.Key,
                i => i.MisfireInstruction,
                i => i.Priority,
                i => i.StartTimeUtc);
        }

        bool TriggerEquals(ISimpleTrigger x, ISimpleTrigger y)
        {
            return TriggerEquals((ITrigger)x, (ITrigger)y) && Equals(x, y,
                i => i.RepeatCount,
                i => i.RepeatInterval);
        }

        bool TriggerEquals(ICalendarIntervalTrigger x, ICalendarIntervalTrigger y)
        {
            return TriggerEquals((ITrigger)x, (ITrigger)y) && Equals(x, y,
                i => i.PreserveHourOfDayAcrossDaylightSavings,
                i => i.RepeatInterval,
                i => i.RepeatIntervalUnit,
                i => i.SkipDayIfHourDoesNotExist,
                i => i.TimeZone);
        }

        bool TriggerEquals(IDailyTimeIntervalTrigger x, IDailyTimeIntervalTrigger y)
        {
            return TriggerEquals((ITrigger)x, (ITrigger)y) && Equals(x, y,
                i => i.DaysOfWeek,
                i => i.EndTimeOfDay,
                i => i.RepeatCount,
                i => i.RepeatInterval,
                i => i.RepeatIntervalUnit,
                i => i.StartTimeOfDay,
                i => i.TimeZone);
        }

        bool TriggerEquals(ICronTrigger x, ICronTrigger y)
        {
            return TriggerEquals((ITrigger)x, (ITrigger)y) && Equals(x, y,
                i => i.CronExpressionString,
                i => i.TimeZone);
        }

        public int GetHashCode(ITrigger obj)
        {
            throw new NotImplementedException();
        }

    }

}
