using JenkinsAccess.EndPoint;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using static JenkinsAccess.EndPoint.JenkinsProject;
using static LanguageExt.Prelude;

namespace PSJenkinsAccess
{
    /// <summary>
    /// Search the Jenkins job log for a particular job or sequence of jobs. The
    /// job object (which contains parameters, id, status) is written to the output.
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "JenkinsJob", DefaultParameterSetName ="minmax")]
    public class FindJenkinsJob : BaseJenkinsJob
    {
        [Parameter(HelpMessage ="Specify a job range, the minimum job of the range. Negative numbers are ok (back from current build). By default it is -50.", ParameterSetName = "minmax")]
        public int? MinimumJobNumber { get; set; }

        [Parameter(HelpMessage = "Specify a job range, the maximum job of the range. Negative numbers are ok (back from the current build number). Default is the current build number.", ParameterSetName = "minmax")]
        public int? MaximumJobNumber { get; set; }

        [Parameter(HelpMessage = "Specify a specific job number", Position = 1, ParameterSetName = "specificJob")]
        public int JobId { get; set; }

        /// <summary>
        /// This parameter makes no sense if you are requesting a specific job - only when fetching a range.
        /// </summary>
        [Parameter(HelpMessage = "What state is the job in?", ParameterSetName = "minmax")]
        public JobStateValue? JobState { get; set; }

        /// <summary>
        /// Process the number of it.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Get the range of job ID's to fetch. The most efficient thing to do is grab the last n (the default is 50
            // when this code was written.
            var r = ParameterSetName == "minmax"
                ? DetermineJobRange(Optional(MinimumJobNumber), Optional(MaximumJobNumber)).Result
                : Right<Exception, IEnumerable<int>>(new[] { JobId });

            // Next, loop over each item and fetch what we need. Or throw if something bad bubbled up.
            r
                .Right(lst => lst.Select(jid => GetJenkinsProject().GetJobBuildInfo(jid)))
                .Left(e => { throw e; })
                .Filter(i => i.Result.Match(j => MatchJob(j), e => false))
                .Iter(i => i.Result
                            .Match(Right: ji => WriteObject(ji), Left: e => { throw e; }));

            // Make sure that PS does its thing.
            base.ProcessRecord();
        }

        /// <summary>
        /// Make sure all filters are applied
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        private bool MatchJob(JenkinsJobBuildInfo j)
        {
            if (JobState.HasValue)
            {
                if (JobState.Value != j.JobState)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Figure out the job range that we are going to look at.
        /// </summary>
        /// <param name="minimumJobNumber"></param>
        /// <param name="maximumJobNumber"></param>
        /// <returns></returns>
        private async Task<Either<Exception, IEnumerable<int>>> DetermineJobRange(Option<int> minimumJobNumber, Option<int> maximumJobNumber)
        {
            // Get the job number when we need it
            var currentJob = new Lazy<Task<Either<Exception, int>>>(async () => await GetJenkinsProject().GetCurrentJobNumber());

            // Next, redo the min and max
            var minJob = minimumJobNumber
                .Some(j => NormalizeJobIndex(j, currentJob))
                .None(() => NormalizeJobIndex(-50, currentJob));
            var maxJob = maximumJobNumber
                .Some(j => NormalizeJobIndex(j, currentJob))
                .None(() => NormalizeJobIndex(0, currentJob));

            // Return an enumerable which will follow the jobs.
            var aminJob = await minJob;
            var amaxJob = await maxJob;
            return from minJ in aminJob
                   from maxJ in amaxJob
                   select Enumerable.Range(minJ, maxJ - minJ + 1);
        }

        /// <summary>
        /// Normalize the job range specified by the user. If the job is less than zero, then assume it is
        /// something relative to the current last job.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="currentJob"></param>
        /// <returns></returns>
        private static async Task<Either<Exception, int>> NormalizeJobIndex(int j, Lazy<Task<Either<Exception, int>>> currentJob)
        {
            return j > 0 ? j : (await currentJob.Value).Map(cj => cj + j);
        }
    }
}
