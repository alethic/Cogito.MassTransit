using System.Threading;
using System.Threading.Tasks;

using Quartz;

namespace Cogito.MassTransit.Scheduler.QuartzIntegration
{

    static class QuartzJobs
    {

        /// <summary>
        /// Gets the stored job, or <c>null</c> if there is none, or if a persistent job store cannot load it. A store
        /// cannot load a job whose type no longer exists, such as one moved into another assembly; the caller then
        /// replaces it.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IJobDetail?> TryGetJobDetail(IScheduler scheduler, JobKey key, CancellationToken cancellationToken)
        {
            try
            {
                return await scheduler.GetJobDetail(key, cancellationToken);
            }
            catch (JobPersistenceException)
            {
                return null;
            }
        }

    }

}
