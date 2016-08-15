using CredentialManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;

namespace JenkinsAccess.EndPoint
{
    /// <summary>
    /// Provide access to the build server using a web client.
    /// </summary>
    class WebClientAccess
    {
        /// <summary>
        /// Fetch info from Jenkins.
        /// </summary>
        /// <param name="jobURIStem"></param>
        /// <returns></returns>
        internal async Task<T> FetchJSON<T>(Uri jobURIStem)
        {
            // update the URI.
            var u = new Uri($"{jobURIStem.OriginalString}/api/json");

            try
            {
                WebClient wc = PreparseWebClient(u);

                // Download the json and parse it.
                var json = await wc.DownloadStringTaskAsync(u);
                return JsonConvert.DeserializeObject<T>(json);

            }
            catch (WebException e) when (e.Message.Contains("Forbidden"))
            {
                throw new ArgumentException($"Jenkins Server seems to require log-in credentials, but we did not find any on this machine that worked. Create a generic credential with '{u.Host}' as the target.", e);
            }
        }

        /// <summary>
        /// Fetch just data as a string
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal async Task<string> FetchData(Uri uri)
        {
            try
            {
                var wc = PreparseWebClient(uri);
                return await wc.DownloadStringTaskAsync(uri);
            }
            catch (WebException e) when (e.Message.Contains("Forbidden"))
            {
                throw new ArgumentException($"Jenkins Server seems to require log-in credentials, but we did not find any on this machine that worked. Create a generic credential with '{uri.Host}' as the target.", e);
            }
        }

        /// <summary>
        /// Post to the Uri directly. We will crash if something goes wrong.
        /// </summary>
        /// <param name="jobUri"></param>
        /// <returns></returns>
        internal async Task Post (Uri jobUri, IDictionary<string,string> parameters = null)
        {
            var wc = PreparseWebClient(jobUri);

            var nv = new System.Collections.Specialized.NameValueCollection();
            if (parameters != null)
            {
                foreach (var nvpair in parameters)
                {
                    nv.Add(nvpair.Key, nvpair.Value);
                }
            }
            var result = await wc.UploadValuesTaskAsync(jobUri, nv);
        }

        /// <summary>
        /// Get a WebClient setup for use.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        private static WebClient PreparseWebClient(Uri u)
        {
            var wc = new WebClient();

            // Do we know about any credentials in this machine for this?
            var sclist = new CredentialSet(u.Host);
            var passwordInfo = sclist.Load().FirstOrDefault();

            // Turns out that since Jenkins never does normal web negotiation, we have to send the credentials right off and
            // encode them by hand. Windows has a PreAuthentication thing, but that requires a bit more work. Pulled this code from
            // the github: https://github.com/tomkuijsten/vsjenkinsmanager/blob/master/Devkoes.VSJenkinsManager/Devkoes.JenkinsManager.APIHandler/Managers/JenkinsDataLoader.cs
            if (passwordInfo != null)
            {
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{passwordInfo.Username}:{passwordInfo.Password}"));
                wc.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            }

            return wc;
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="artifactUri"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        internal async Task DownloadFile(Uri artifactUri, FileInfo destination)
        {
            // Make sure we are ready.
            if (!destination.Directory.Exists)
            {
                destination.Directory.Create();
            }

            // Do the download.
            var wc = PreparseWebClient(artifactUri);

            bool gotFile = false;
            try
            {
                await wc.DownloadFileTaskAsync(artifactUri, destination.FullName);
                gotFile = true;
            }
            catch (WebException e) when (e.Message.Contains("Forbidden"))
            {
                throw new ArgumentException($"Jenkins Server seems to require log-in credentials, but we did not find any on this machine that worked. Create a generic credential with '{artifactUri.Host}' as the target.", e);
            }
            finally
            {
                if (!gotFile)
                {
                    // Clean up if a partial file has been left behind.
                    destination.Refresh();
                    if (destination.Exists)
                    {
                        destination.Delete();
                        destination.Refresh();
                    }
                }
            }
        }
    }
}
