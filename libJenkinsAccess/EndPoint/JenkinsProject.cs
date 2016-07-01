using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using static JenkinsAccess.Data.JenkinsDomain;

namespace JenkinsAccess.EndPoint
{
    public class JenkinsProject
    {
        /// <summary>
        /// Hold onto the job we need.
        /// </summary>
        private Option<string> _jobName;

        /// <summary>
        /// Hold onto a server.
        /// </summary>
        private JenkinsServer _server;

        /// <summary>
        /// Initalize the project reference.
        /// </summary>
        /// <param name="projectUri"></param>
        public JenkinsProject(Uri projectUri)
        {
            this._jobName = projectUri.JenkinsJobName();
            this._server = new JenkinsServer(projectUri);
        }

        /// <summary>
        /// Return the current build number from the last build.
        /// </summary>
        /// <returns></returns>
        public async Task<Either<Exception,int>> GetCurrentJobNumber()
        {
            // Build the job URI, which will then return the JSON.
            var jk = GetJobURIStem()
                .Some(async v => await _server.FetchJSON<JenkinsJob>(v))
                .None(() => Left<Exception,JenkinsJob>(new InvalidOperationException("No valid stem - should never happen!")).AsTask());

            var r = (await jk)
                .Right(j => j.lastBuild == null ? Left<Exception, JenkinsJob>(new ArgumentException("No build completed")) : Right<Exception, JenkinsJob>(j))
                .Left(e => Left<Exception, JenkinsJob>(e))
                .Map(j => j.lastBuild.number);

            return r;
        }

        /// <summary>
        /// Create the URI stem.
        /// </summary>
        /// <returns></returns>
        private Option<Uri> GetJobURIStem()
        {
            return _jobName.Map(s => _server.GetUriWithStem($"job/{s}"));
        }

#if false
        /// <summary>
        /// Info on an artifact
        /// </summary>
        public class JenkinsArtifact
        {
            public string JobName;
            public string ArtifactName;
            public int BuildNumber;
        }

                    _artifactURI = url;
            var segments = _artifactURI.Segments;

            // Get the job and artifact.
            var artifactInfo = segments.SkipWhile(s => s != "job/").Skip(1).Select(s => s.Trim('/')).ToArray();
            if (artifactInfo.Length != 4 && artifactInfo[2] == "artifact")
            {
                throw new ArgumentException($"The Jenkins artifact URI '{url}' is not in a format I recognize (.../jobname/build/artifact/artifact-name)");
            }
            _jobName = artifactInfo[0];
            _buildName = artifactInfo[1];
            _artifactName = artifactInfo[3];
#endif

    }
}
