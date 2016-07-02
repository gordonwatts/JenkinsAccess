using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSJenkinsAccess
{
    [Cmdlet(VerbsCommon.Get, "JenkinsBuildLogfile")]
    public class GetJobLogFile : BaseJobBuildCmdlet
    {
        /// <summary>
        /// Grab the log file and send it out as a giant text file.
        /// </summary>
        protected override void ProcessRecord()
        {
            GetJenkinsProject().GetJobLogfile(GetJobId())
                .Result
                .Right(v => { foreach (var l in v.Replace("\r\n", "\n").Split('\n')) { WriteObject(l); } })
                .Left(e => { throw e; });

            base.ProcessRecord();
        }
    }
}
