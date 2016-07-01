using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace JenkinsAccess
{
    /// <summary>
    /// Generic extension functions ot help get things done.
    /// </summary>
    public static class __utils
    {
        /// <summary>
        /// Gets a job name, which comes on the item after the "job" in the name.
        /// </summary>
        /// <param name="jobUri"></param>
        /// <returns></returns>
        public static Option<string> JenkinsJobName (this Uri jobUri)
        {
            return Optional(
                jobUri
                .Segments
                .SkipWhile(seg => seg != "job/")
                .Skip(1)
                .FirstOrDefault()
                )
                .Map(s => s.Trim('/'));
        }
    }
}
