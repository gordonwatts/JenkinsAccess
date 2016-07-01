using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using static JenkinsAccess.Data.JenkinsDomain;
using JenkinsAccess.Data;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

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
            // TODO: See https://github.com/louthy/language-ext/issues/101 for cleaning this up.
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
        /// Fetch from the server the info needed.
        /// </summary>
        /// <param name="jobid"></param>
        /// <returns></returns>
        /// <remarks>
        /// Look at the cache first
        /// </remarks>
        public async Task<Either<Exception, JenkinsJobBuildInfo>> GetJobBuildInfo(int jobid)
        {
            // Fetch the raw information.
            var jinfo = (await LoadFromCache(jobid))
                .Some(jvi => Right<Exception, JenkinsJobBuild>(jvi).AsTask())
                .None(() => GetJobURIStem()
                    .Some(v => _server.FetchJSON<JenkinsJobBuild>(new Uri($"{v.AbsoluteUri}/{jobid}")))
                    .None(() => Left<Exception, JenkinsJobBuild>(new InvalidOperationException("No job uri - should never happen.")).AsTask())
                );
            var r = await jinfo;
            await r.MatchAsync(async jvi => await SaveToCache(jvi), e => new JenkinsJobBuild().AsTask());

            // Convert it to our summary information
            return r
                .Map(jvi => new JenkinsJobBuildInfo() {
                    Id = jvi.number,
                    IsBuilding = jvi.building,
                    Parameters = ConvertParameters(jvi.actions),
                    JobUrl = new Uri(jvi.url)
                });
        }

        /// <summary>
        /// Convert the Jenkins parameters to something more resonable.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        private Dictionary<string, string> ConvertParameters(List<Data.Action> actions)
        {
            return actions
                .Where(a => a.parameters != null)
                .SelectMany(a => a.parameters)
                .ToDictionary(p => p.name, p => p.value);
        }

        /// <summary>
        /// See if we can load a job item from the cache.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        private async Task<Option<JenkinsJobBuild>> LoadFromCache(int jobId)
        {
            var f = GetCacheFile(jobId);
            if (!f.Exists)
            {
                return None;
            }

            using (var rdr = f.OpenText())
            {
                var obj = JsonConvert.DeserializeObject<JenkinsJobBuild>(await rdr.ReadToEndAsync());
                return Some(obj);
            }
        }

        /// <summary>
        /// Write an item to cache if it is done building.
        /// </summary>
        /// <param name="bld"></param>
        /// <returns></returns>
        private async Task<JenkinsJobBuild> SaveToCache(JenkinsJobBuild bld)
        {
            // If building, then don't cache.
            if (bld.building)
            {
                return bld;
            }

            // Create key, and write out.
            using (var wrtr = GetCacheFile(bld.number).CreateText())
            {
                await wrtr.WriteAsync(JsonConvert.SerializeObject(bld));
            }

            return bld;
        }

        private FileInfo GetCacheFile(int number)
        {
            var d = new FileInfo(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"JenkinsCache/{_jobName}/{number}-status.json"));
            if (!d.Directory.Exists)
            {
                d.Directory.Create();
            }
            return d;
        }

        /// <summary>
        /// Info for a job
        /// </summary>
        public class JenkinsJobBuildInfo
        {
            public int Id { get; internal set; }
            public bool IsBuilding { get; internal set; }

            /// <summary>
            /// Url pointing to the job.
            /// </summary>
            public Uri JobUrl { get; internal set; }
            public Dictionary<string, string> Parameters { get; internal set; }
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
