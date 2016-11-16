using LanguageExt;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static JenkinsAccess.Data.JenkinsDomain;

namespace JenkinsAccess.EndPoint
{
    public class JenkinsServer
    {
        /// <summary>
        /// Access to Jenkins REST API
        /// </summary>
        Lazy<WebClientAccess> _JenkinsEndPoint = new Lazy<WebClientAccess>(() => new WebClientAccess());

        /// <summary>
        /// Uri of server.
        /// </summary>
        Uri _serverUri;

        /// <summary>
        /// Get the server address we are looking at
        /// </summary>
        /// <param name="url"></param>
        public JenkinsServer(Uri url)
        {
            var goodSegment = url.Segments.SkipUntilLast(s => s == "job/").Count();
            if (!url.Segments.Last().EndsWith("/"))
            {
                goodSegment--;
            }
            var backup = Enumerable.Range(0, goodSegment).Aggregate("", (a, v) => a + "../");
            _serverUri = new Uri(url, backup);
        }

        /// <summary>
        /// Return a new Uri.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        internal Uri GetUriWithStem(string stem)
        {
            return new Uri(_serverUri, stem);
        }

        /// <summary>
        /// Return the object requested from the uri from the jenkins server, or return the error.
        /// Don't fail, just return it - let the folks upstairs decide how to deal with the error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal async Task<Either<Exception,T>> FetchJSON<T>(Uri uri)
        {
            try
            {
                return await _JenkinsEndPoint.Value.FetchJSON<T>(uri);
            } catch(Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Fetch a string from the remote server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal async Task<Either<Exception, string>> FetchData(Uri url)
        {
            try
            {
                return await _JenkinsEndPoint.Value.FetchData(url);
            }
            catch (Exception e)
            {
                return e;
            }
        }
        
        /// <summary>
        /// Downlaod data to a local file.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        internal async Task<Either<Exception, FileInfo>> DownlaodData (Uri url, FileInfo destination)
        {
            try
            {
                await _JenkinsEndPoint.Value.DownloadFile(url, destination);
                return destination;
            }
            catch (Exception e)
            {
                return e;
            }
        }

#if false
        /// <summary>
        /// Fetch the last successful build.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetLastSuccessfulBuild()
        {
            // Build the job URI, which will then return the JSON.
            var jobURIStem = GetJobURIStem();
            var r = await _JenkinsEndPoint.Value.FetchJSON<JenkinsJob>(jobURIStem);

            if (r.lastSuccessfulBuild == null)
            {
                throw new InvalidOperationException($"This Jenkins job does not yet have a successful build! {jobURIStem.OriginalString}");
            }

            return r.lastSuccessfulBuild.number;
        }

        /// <summary>
        /// Return a Uri of the job stem.
        /// </summary>
        /// <returns></returns>
        private Uri GetJobURIStem()
        {
            return new Uri(_artifactURI.OriginalString.Substring(0, _artifactURI.OriginalString.IndexOf(_jobName) + _jobName.Length));
        }

        /// <summary>
        /// Parse the URL to figure out everything we need
        /// </summary>
        /// <returns></returns>
        public async Task<Info> GetArtifactInfo()
        {
            await Init();
            return new Info()
            {
                JobName = _jobName,
                BuildNumber = int.Parse(_buildName),
                ArtifactName = _artifactName
            };
        }

        /// <summary>
        /// Download the artifact!
        /// </summary>
        /// <param name="artifactInfo"></param>
        /// <returns></returns>
        internal async Task Download(Info artifactInfo, FileInfo destination)
        {
            // Build the url
            var jobURI = GetJobURIStem();
            var artifactUri = new Uri($"{jobURI.OriginalString}/{artifactInfo.BuildNumber}/artifact/{artifactInfo.ArtifactName}");

            await _JenkinsEndPoint.Value.DownloadFile(artifactUri, destination);
        }
#endif
    }
}
