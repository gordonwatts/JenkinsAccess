using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PSJenkinsAccess
{
    [Cmdlet(VerbsLifecycle.Invoke, "JenkinsJob")]
    public class InvokeJenkinsJob : PSCmdlet
    {
        /// <summary>
        /// The uri of the job.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The URL of the Jenkins job - visit the job page on the web and use that URL.")]
        public string JobUri { get; set; }

        /// <summary>
        /// Run a job
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteObject("hi");
            base.ProcessRecord();
        }
    }
}
