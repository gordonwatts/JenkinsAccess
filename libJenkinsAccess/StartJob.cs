using JenkinsAccess.EndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JenkinsAccess
{
    /// <summary>
    /// Start a job with parameters (or similar).
    /// </summary>
    public class StartJob
    {
        /// <summary>
        /// Start a build.
        /// </summary>
        /// <param name="jobUri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task StartJenkinsJob (Uri jobUri, IDictionary<string,string> parameters)
        {
            var invokeBuildURI = parameters == null || parameters.Count == 0
                ? new Uri(jobUri, "build")
                : new Uri(jobUri, "buildWithParameters");

            var wca = new WebClientAccess();
            await wca.Post(invokeBuildURI, parameters, new[] { await GetBuildCrumb(jobUri) });
        }

        /// <summary>
        /// Submit a job for re-build.
        /// </summary>
        /// <param name="buildUri">The URI for the specific build we want to re-start</param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public static async Task RebuildJenkinsJob (Uri buildUri)
        {
            var reInvokeUri = new Uri(buildUri, "rebuild");
            var wca = new WebClientAccess();
            var r = await wca.FetchData(reInvokeUri);
        }

        /// <summary>
        /// Return the build crumb for the job.
        /// </summary>
        /// <param name="jobUri">The job uri from which we can extract the server</param>
        /// <returns></returns>
        public static async Task<(string, string)> GetBuildCrumb(Uri jobUri)
        {
            var curi = new Uri(jobUri, "/crumbIssuer/api/xml?xpath=concat(//crumbRequestField,%22:%22,//crumb)");
            var wca = new WebClientAccess();
            var crumbData = await wca.FetchData(curi);
            var ds = crumbData.Split(':');
            if (ds.Length != 2)
            {
                throw new ArgumentException($"Crumb from the Jenkins server didn't come back in a key:value format: '{crumbData}'");
            }
            return (ds[0], ds[1]);
        }
    }
}
