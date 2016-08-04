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

        #region Log File
        /// <summary>
        /// Return a string that is the total contents of the log file. Could be large!
        /// </summary>
        /// <param name="jobid"></param>
        /// <returns></returns>
        public async Task<Either<Exception, string>> GetJobLogfile(int jobid)
        {
            var jinfo = (await LogFileLoadFromCache(jobid))
                .Some(data => Right<Exception, string>(data).AsTask())
                .None(() => GetJobURIStem()
                            .Some(v => _server.FetchData(new Uri($"{v.AbsoluteUri}/{jobid}/consoleText")))
                            .None(() => Left<Exception, string>(new InvalidOperationException("No job url - should never happen.")).AsTask())
                            );

            var r = await jinfo;
            await r.MatchAsync(rdata => LogFileSaveToCache(jobid, rdata), e => "hi".AsTask());
            return r;
        }


        /// <summary>
        /// Save a file to cache
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="r"></param>
        private async Task<string> LogFileSaveToCache(int jobid, string r)
        {
            await SaveCacheFile(jobid, "logfile", r);
            return r;
        }

        /// <summary>
        /// Load a log file from the cache
        /// </summary>
        /// <param name="jobid"></param>
        /// <returns></returns>
        private Task<Option<string>> LogFileLoadFromCache(int jobid)
        {
            return LoadCacheFile(jobid, "logfile");
        }
        #endregion

        #region Artifacts
        /// <summary>
        /// Fetch an artifact from Jenkins and download locally
        /// </summary>
        /// <param name="artifactName"></param>
        /// <returns></returns>
        public Task<Either<Exception,FileInfo>> GetArtifact(int jobID, string artifactName)
        {
            var cacheFile = GetCacheFilename(jobID, artifactName);
            if (!cacheFile.Exists)
            {
                // Fetch it.
                var output = GetJobURIStem()
                    .Some(v => _server.DownlaodData(new Uri($"{v.AbsoluteUri}/{jobID}/artifact/{artifactName}"), cacheFile))
                    .None(() => Left<Exception, FileInfo>(new InvalidOperationException("No job url - should never happen.")).AsTask());
                return output;
            }

            return Right<Exception, FileInfo>(cacheFile).AsTask();
        }
        #endregion

        #region BuildInfo cache file
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
            var jinfo = (await BuildInfoLoadFromCache(jobid))
                .Some(jvi => Right<Exception, JenkinsJobBuild>(jvi).AsTask())
                .None(() => GetJobURIStem()
                    .Some(v => _server.FetchJSON<JenkinsJobBuild>(new Uri($"{v.AbsoluteUri}/{jobid}")))
                    .None(() => Left<Exception, JenkinsJobBuild>(new InvalidOperationException("No job uri - should never happen.")).AsTask())
                );
            var r = await jinfo;
            await r.MatchAsync(async jvi => await BuildInfoSaveToCache(jvi), e => new JenkinsJobBuild().AsTask());

            // Convert it to our summary information
            return r
                .Map(jvi => new JenkinsJobBuildInfo()
                {
                    Id = jvi.number,
                    IsBuilding = jvi.building,
                    Parameters = ConvertParameters(jvi.actions),
                    Status = jvi.result,
                    Changes = jvi.changeSet.items.Count != 0,
                    JobState = jvi.building
                                ? JobStateValue.Running
                                : jvi.result == "SUCCESS"
                                ? JobStateValue.Success
                                : JobStateValue.Failure,
                    JobUrl = new Uri(jvi.url),
                    RebuildsJob = jvi.actions.Where(a => a?.causes != null).SelectMany(a => a?.causes).Where(c => c?.upstreamBuild != null).Select(c => c.upstreamBuild).FirstOrDefault()
                });
        }

        /// <summary>
        /// Convert the Jenkins parameters to something more resonable.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        private Hashtable ConvertParameters(List<Data.Action> actions)
        {
            return actions
                .Where(a => a.parameters != null)
                .SelectMany(a => a.parameters)
                .ToDictionary(p => p.name, p => p.value)
                .ToHashtable();
        }

        /// <summary>
        /// See if we can load a job item from the cache.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        private async Task<Option<JenkinsJobBuild>> BuildInfoLoadFromCache(int jobId)
        {
            return (await LoadCacheFile(jobId, "buildinfo"))
                .Map(s => JsonConvert.DeserializeObject<JenkinsJobBuild>(s));
        }

        /// <summary>
        /// Write an item to cache if it is done building.
        /// </summary>
        /// <param name="bld"></param>
        /// <returns></returns>
        private async Task<JenkinsJobBuild> BuildInfoSaveToCache(JenkinsJobBuild bld)
        {
            // If building, then don't cache.
            if (bld.building)
            {
                return bld;
            }

            await SaveCacheFile(bld.number, "buildinfo", JsonConvert.SerializeObject(bld));
            return bld;
        }
        #endregion

        #region Lowlevel Cache Routines
        /// <summary>
        /// Load a cache file from text. Return None if there is no text file.
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="cacheFilename"></param>
        /// <returns></returns>
        private async Task<Option<string>> LoadCacheFile(int jobid, string cacheFilename)
        {
            // See if we have a cache hit.
            var f = GetCacheFilename(jobid, cacheFilename);
            if (!f.Exists)
            {
                return None;
            }

            // Read it back.
            using (var r = f.OpenText())
            {
                return Some(await r.ReadToEndAsync());
            }
        }

        /// <summary>
        /// Generic method to save a text file in our cache.
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="dataToCache"></param>
        /// <returns></returns>
        private async Task SaveCacheFile(int jobid, string cacheFileName, string dataToCache)
        {
            using (var wrtr = GetCacheFilename(jobid, cacheFileName).CreateText())
            {
                await wrtr.WriteAsync(dataToCache);
            }
        }

        /// <summary>
        /// Generate the filename for a cache file for a job. Make sure the directory exists first.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="cacheFileName"></param>
        /// <returns></returns>
        private FileInfo GetCacheFilename(int number, string cacheFileName)
        {
            var extension = cacheFileName.Contains(".") ? "" : ".json";
           var d = _jobName
                .Match(s => new FileInfo(Path.Combine(Path.GetTempPath(), $"JenkinsCache/{s}/{number}-{cacheFileName}{extension}")),
                () => { throw new InvalidOperationException(); });
            if (!d.Directory.Exists)
            {
                d.Directory.Create();
            }
            return d;
        }
        #endregion

        /// <summary>
        /// Current state of the job.
        /// </summary>
        public enum JobStateValue
        {
            Success,
            Failure,
            Running
        }

        /// <summary>
        /// Info for a job.
        /// </summary>
        /// <remarks>This is designed to be a PowerShell friendly object.</remarks>
        public class JenkinsJobBuildInfo
        {
            public int Id { get; internal set; }
            public bool IsBuilding { get; internal set; }

            /// <summary>
            /// Url pointing to the job.
            /// </summary>
            public Uri JobUrl { get; internal set; }

            /// <summary>
            /// The parameter key/value pairs. They are all strings.
            /// </summary>
            public Hashtable Parameters { get; internal set; }

            /// <summary>
            /// The final status of the Jenkins job.
            /// </summary>
            public string Status { get; internal set; }

            /// <summary>
            /// True if there were changes made to the source code being built.
            /// </summary>
            public bool Changes { get; internal set; }

            /// <summary>
            /// The current state of the job
            /// </summary>
            public JobStateValue JobState { get; internal set; }

            /// <summary>
            /// If set, points to the job number that this job is rebuilding
            /// </summary>
            public int? RebuildsJob { get; internal set; }
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
