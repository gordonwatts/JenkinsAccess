using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Classes that we get back when we fetch a job build.
/// </summary>
namespace JenkinsAccess.Data
{
    /// <summary>
    /// What triggered the build?
    /// </summary>
    public class Cause
    {
        public string shortDescription { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public int? upstreamBuild { get; set; }
        public string upstreamProject { get; set; }
        public string upstreamUrl { get; set; }
    }

    /// <summary>
    /// Build parameter
    /// </summary>
    public class Parameter
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Branch
    {
        public string SHA1 { get; set; }
        public string name { get; set; }
    }

    public class Marked
    {
        public string SHA1 { get; set; }
        public List<Branch> branch { get; set; }
    }

    public class Revision
    {
        public string SHA1 { get; set; }
        public List<Branch> branch { get; set; }
    }

    public class RefsRemotesOriginMaster
    {
        public int buildNumber { get; set; }
        public object buildResult { get; set; }
        public Marked marked { get; set; }
        public Revision revision { get; set; }
    }

    public class BuildsByBranchName
    {
        public RefsRemotesOriginMaster refs { get; set; }
    }

    public class LastBuiltRevision
    {
        public string SHA1 { get; set; }
        public List<Branch> branch { get; set; }
    }

    public class Action
    {
        public List<Cause> causes { get; set; }
        public List<Parameter> parameters { get; set; }
        public BuildsByBranchName buildsByBranchName { get; set; }
        public LastBuiltRevision lastBuiltRevision { get; set; }
        public List<string> remoteUrls { get; set; }
        public string scmName { get; set; }
    }

    public class Artifact
    {
        public string displayPath { get; set; }
        public string fileName { get; set; }
        public string relativePath { get; set; }
    }

    public class ChangeSet
    {
        public List<object> items { get; set; }
        public string kind { get; set; }
    }

    public class JenkinsJobBuild
    {
        public List<Action> actions { get; set; }
        public List<Artifact> artifacts { get; set; }
        public bool building { get; set; }
        public object description { get; set; }
        public string displayName { get; set; }
        public int duration { get; set; }
        public int estimatedDuration { get; set; }
        public object executor { get; set; }
        public string fullDisplayName { get; set; }
        public string id { get; set; }
        public bool keepLog { get; set; }
        public int number { get; set; }
        public int queueId { get; set; }
        public string result { get; set; }
        public long timestamp { get; set; }
        public string url { get; set; }
        public string builtOn { get; set; }
        public ChangeSet changeSet { get; set; }
        public List<object> culprits { get; set; }
    }
}