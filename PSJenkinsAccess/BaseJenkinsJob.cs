using JenkinsAccess.EndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSJenkinsAccess
{
    /// <summary>
    /// Provide some base services
    /// </summary>
    public class BaseJenkinsJob : PSCmdlet
    {
        /// <summary>
        /// The uri of the job.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The URL of the Jenkins job - visit the job page on the web and use that URL.")]
        public string JobUri { get; set; }

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
