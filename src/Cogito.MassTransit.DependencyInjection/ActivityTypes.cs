using System;
using System.Linq;

using MassTransit;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Finds the argument and log types of a routing slip activity.
    /// </summary>
    static class ActivityTypes
    {

        /// <summary>
        /// Gets the arguments type of the activity, from the <see cref="IExecuteActivity{TArguments}"/> it implements.
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        public static Type GetArgumentsType(Type activityType) => GetClosingArgument(activityType, typeof(IExecuteActivity<>));

        /// <summary>
        /// Gets the log type of the activity, from the <see cref="ICompensateActivity{TLog}"/> it implements.
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        public static Type GetLogType(Type activityType) => GetClosingArgument(activityType, typeof(ICompensateActivity<>));

        static Type GetClosingArgument(Type type, Type openInterface)
        {
            var closed = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterface).ToList();
            if (closed.Count != 1)
                throw new ArgumentException($"{type} must implement {openInterface.Name} exactly once.", nameof(type));

            return closed[0].GetGenericArguments()[0];
        }

    }

}
