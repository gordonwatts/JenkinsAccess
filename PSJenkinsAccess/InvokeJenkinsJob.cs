using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading.Tasks;
using JenkinsAccess;
using System.Collections;
using static JenkinsAccess.EndPoint.JenkinsProject;
using JenkinsAccess.EndPoint;

namespace PSJenkinsAccess
{
    [Cmdlet(VerbsLifecycle.Invoke, "JenkinsJob")]
    public class InvokeJenkinsJob : PSCmdlet
    {
        /// <summary>
        /// The uri of the job.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The URL of the Jenkins job - visit the job page on the web and use that URL.", ParameterSetName = "ExplicitParams")]
        public string JobUri { get; set; }

        /// <summary>
        /// And the parameters to invoke with
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, ValueFromPipeline = true, ParameterSetName = "ExplicitParams")]
        public Hashtable ParameterValues { get; set; }

        /// <summary>
        /// Reinvoke a job given this  guy
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ParameterSetName = "reinvoke")]
        public JenkinsJobBuildInfo JobInfo { get; set; }

        /// <summary>
        /// Start or rebuild a job
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                var dict = ParameterSetName == "ExplicitParams"
                    ? (ParameterValues == null
                            ? new Dictionary<string, string>()
                            : ParameterValues.Keys.Cast<string>().ToDictionary(k => k, k => (string)ParameterValues[k]))
                    : JobInfo.Parameters.ToDictionary();

                // The job uri should always end with a "/".
                if (!JobUri.EndsWith("/"))
                {
                    JobUri += "/";
                }

                // Run the job!
                StartJob.StartJenkinsJob(new Uri(ParameterSetName == "ExplicitParams" ? JobUri : JobInfo.JobUrl.RemoveBuildReference().OriginalString), dict).Wait();
            } catch (AggregateException e)
            {
                if (e.InnerExceptions.Count == 1)
                {
                    throw e.InnerException;
                }
                throw;
            }
            base.ProcessRecord();
        }

        /// <summary>
        /// Cache the project when we are able to (and it make sense).
        /// </summary>
        private JenkinsProject _project = null;

        /// <summary>
        /// Return a server URI.
        /// </summary>
        /// <returns></returns>
        internal JenkinsProject GetJenkinsProject()
        {
            if (_project == null)
            {
                _project = new JenkinsProject(new Uri(JobUri));
            }
            return _project;
        }
    }
}
