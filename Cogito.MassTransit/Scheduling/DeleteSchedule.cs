namespace Cogito.MassTransit.Scheduling
{

    /// <summary>
    /// Request to delete a scheduled message.
    /// </summary>
    public class DeleteSchedule
    {

        /// <summary>
        /// Unique identifying name to categorize this schedule.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Unique identifying name of the schedule within the group.
        /// </summary>
        public string Name { get; set; }

    }

}
