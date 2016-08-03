using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSJenkinsAccess
{
    /// <summary>
    /// Fetch an artifact from a job. Caches it locally and writes out the address to the cache
    /// out of its pipe.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "JenkinsArtifact")]
    public class GetJobArtifact : BaseJobBuildCmdlet
    {
        [Parameter(HelpMessage = "The name of the artifact to return a local path to.", Mandatory = true, ValueFromPipeline = true)]
        public string ArtifactName { get; set; }

        /// <summary>
        /// Grab the log file and send it out as a giant text file.
        /// </summary>
        protected override void ProcessRecord()
        {
            GetJenkinsProject().GetArtifact(JobId, ArtifactName)
                .Result
                .Right(v => { WriteObject(v); })
                .Left(e => { throw e; });

            base.ProcessRecord();
        }
    }
}
