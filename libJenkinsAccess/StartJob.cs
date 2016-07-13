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
            await wca.Post(invokeBuildURI, parameters);
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
    }
}
