using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading.Tasks;
using JenkinsAccess;
using System.Collections;

namespace PSJenkinsAccess
{
    [Cmdlet(VerbsLifecycle.Invoke, "JenkinsJob")]
    public class InvokeJenkinsJob : PSCmdlet
    {
        /// <summary>
        /// The uri of the job.
        /// </summary>
        [Parameter(Mandatory = true, Position =1, HelpMessage = "The URL of the Jenkins job - visit the job page on the web and use that URL.")]
        public string JobUri { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipeline = true)]
        public Hashtable ParameterValues { get; set; }

        /// <summary>
        /// Run a job
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                var dict = ParameterValues == null
                    ? new Dictionary<string, string>()
                    : ParameterValues.Keys.Cast<string>().ToDictionary(k => k, k => (string)ParameterValues[k]);
                StartJob.StartJenkinsJob(new Uri(JobUri), dict).Wait();
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
    }
}
