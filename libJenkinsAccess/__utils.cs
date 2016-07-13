using LanguageExt;
using System;
using System.Collections;
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

        /// <summary>
        /// Given a hash table, create a dict of strings.
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this Hashtable h)
        {
            var r = new Dictionary<string, string>();
            foreach (var k in h.Keys)
            {
                r[k as string] = h[k] as string;
            }

            return r;
        }

        /// <summary>
        /// Given a URI pointing at somethign job releated, get out the job only URI.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Uri RemoveBuildReference (this Uri original)
        {
            var jname = original.Segments.SkipWhile(j => j != "job/").Skip(1).FirstOrDefault();
            if (jname == null)
                return original;

            return new Uri(original.OriginalString.Substring(0, original.OriginalString.IndexOf(jname) + jname.Length));
        }
    }
}
