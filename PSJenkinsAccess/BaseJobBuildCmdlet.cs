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
        /// <summary>
        /// Specifiy the job as a complete object. Contains everything we need.
        /// </summary>
        [Parameter(HelpMessage = "The object from a Find-JenkinsJob", Position = 1, Mandatory = true, ValueFromPipeline =true, ParameterSetName = "JobAsProperty")]
        public JenkinsJobBuildInfo JobInfo { get; set; }

        [Parameter(HelpMessage = "Specify the build number", Position = 1, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "JobAsIdUri")]
        public int JobId { get; set; }

        /// <summary>
        /// The URI
        /// </summary>
        [Parameter(HelpMessage = "URI of the job web page", ParameterSetName = "JobAsIdUri", Mandatory = true)]
        public string JobUri { get; set; }

        /// <summary>
        /// Cache the project. Not expected to change as we go.
        /// </summary>
        private JenkinsProject _project = null;

        /// <summary>
        /// Extract the build job project object from the input parameters.
        /// </summary>
        /// <returns></returns>
        internal JenkinsProject GetJenkinsProject()
        {
            if (_project == null)
            {
                if (ParameterSetName == "JobAsIdUri")
                {
                    _project = new JenkinsProject(new Uri(JobUri));
                }
                else
                {
                    _project = new JenkinsProject(JobInfo.JobUrl);
                }
            }
            return _project;
        }

        /// <summary>
        /// Extract the job from the parameters.
        /// </summary>
        /// <returns></returns>
        internal int GetJobId()
        {
            if (ParameterSetName == "JobAsIdUri")
            {
                return JobId;
            }
            else
            {
                return JobInfo.Id;
            }
        }
    }
}
