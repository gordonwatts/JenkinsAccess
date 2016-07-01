using JenkinsAccess.EndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using static JenkinsAccess.EndPoint.JenkinsProject;

namespace PSJenkinsAccess
{
    /// <summary>
    /// Base job when looking at a particular job
    /// </summary>
    public class BaseJobBuildCmdlet : PSCmdlet
    {
        [Parameter(HelpMessage = "The object from a Find-JenkinsJob", ValueFromPipeline =true)]
        public JenkinsJobBuildInfo JobInfo { get; set; }

        private JenkinsProject _project = null;

        /// <summary>
        /// Extract the build job project object from the input parameters.
        /// </summary>
        /// <returns></returns>
        internal JenkinsProject GetJenkinsProject()
        {
            if (_project == null)
            {
                _project = new JenkinsProject(JobInfo.JobUrl);
            }
            return _project;
        }

        private int _jobId = -1;

        /// <summary>
        /// Extract the job from the parameters.
        /// </summary>
        /// <returns></returns>
        internal int GetJobId()
        {
            if (_jobId <= 0)
            {
                _jobId = JobInfo.Id;
            }
            return _jobId;
        }

        protected override void ProcessRecord()
        {
            _jobId = -1;
            base.ProcessRecord();
        }
    }
}
