using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Configuration;
using EPiServer.Logging;
using EPiServer.Web.Routing;

using ImageProcessor.Web.Episerver;
using ImageProcessor.Web.Caching;
//using ImageProcessor.Web.HttpModules;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageProcessor.Web.Episerver.Azure
{
    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses the Episerver configured Azure blob storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class AzureBlobCache : ImageCacheBase
    {
        /// <summary>
        /// The assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = typeof(EpiserverImageProcessingModule).Assembly.GetName().Version.ToString();

        /// <summary>
        /// Use the configured logging mechanism
        /// </summary>
        private static readonly ILogger logger = LogManager.GetLogger();

        /// <summary>
        /// The cloud cached root blob container.
        /// </summary>
        private static CloudBlobContainer rootContainer;

        /// <summary>
        /// Determines if the CDN request is redirected or rewritten
        /// </summary>
        private readonly bool streamCachedImage;

        /// <summary>
        /// The timeout length for requesting the url.
        /// </summary>
        private readonly int timeout = 1000;

        public string blobPath;

        private const string prefix = "3p!_";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobCache"/> class.
        /// </summary>
        /// <param name="requestPath">
        /// The request path for the image.
        /// </param>
        /// <param name="fullPath">
        /// The full path for the image.
        /// </param>
        /// <param name="querystring">
        /// The query string containing instructions.
        /// </param>
        public AzureBlobCache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            // Get the name of the configured blob provider
            string providerName = EPiServerFrameworkSection.Instance.Blob.DefaultProvider;


            if (rootContainer == null)
            {
                // Get the name of the connection string from there
                string connectionStringName = EPiServerFrameworkSection.Instance.Blob.Providers[providerName].Parameters["connectionStringName"];

                // Retrieve storage account from connection string.
                CloudStorageAccount cloudCachedStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
                // Create the blob client.
                CloudBlobClient cloudBlobClient = cloudCachedStorageAccount.CreateCloudBlobClient();


                // Retrieve reference to the container. Container is already created as part of the initialization process for the BlobProvider
                rootContainer = cloudBlobClient.GetContainerReference(EPiServerFrameworkSection.Instance.Blob.Providers[providerName].Parameters["container"]);
            }

            //cachedCdnRoot = Settings.ContainsKey("CachedCDNRoot")
            //                         ? Settings["CachedCDNRoot"]
            //                         : rootContainer.Uri.ToString().TrimEnd(rootContainer.Name.ToCharArray());

            if (Settings.ContainsKey("CDNTimeout"))
            {
                int.TryParse(Settings["CDNTimeout"], out int t);
                timeout = t;
            }

            // This setting was added to facilitate streaming of the blob resource directly instead of a redirect. This is beneficial for CDN purposes
            // but caution should be taken if not used with a CDN as it will add quite a bit of overhead to the site.
            // See: https://github.com/JimBobSquarePants/ImageProcessor/issues/161
            streamCachedImage = Settings.ContainsKey("StreamCachedImage") && Settings["StreamCachedImage"].ToLower() == "true";
        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> returning the value.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            string cachedFilename = prefix + await CreateCachedFileNameAsync();

            var media = UrlResolver.Current.Route(new UrlBuilder(FullPath)) as MediaData;

            string containerName = media?.BinaryDataContainer?.Segments[1];

            if (containerName == null)
            {
                // We're working with a static file here
                containerName = $"_{prefix}static";
            }

            blobPath = $"{containerName}/{cachedFilename}";

            CachedPath = $"{rootContainer.Uri.ToString()}/{containerName}/{cachedFilename}";

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(CachedPath);

            if (cachedImage == null)
            {
                CloudBlockBlob blockBlob = rootContainer.GetBlockBlobReference(blobPath);
                //string t = GetSaSForBlob(blockBlob, SharedAccessBlobPermissions.Read);

                if (await blockBlob.ExistsAsync())
                {
                    // Pull the latest info.
                    await blockBlob.FetchAttributesAsync();

                    if (blockBlob.Properties.LastModified.HasValue)
                    {
                        cachedImage = new CachedImage
                        {
                            Key = Path.GetFileNameWithoutExtension(CachedPath),
                            Path = CachedPath,
                            CreationTimeUtc = blockBlob.Properties.LastModified.Value.UtcDateTime
                        };

                        CacheIndexer.Add(cachedImage, ImageCacheMaxMinutes);
                    }
                }
            }

            if (cachedImage == null)
            {
                // Nothing in the cache so we should return true.
                isUpdated = true;
            }
            else
            {
                // Check to see if the cached image is set to expire
                // or a new file with the same name has replaced our current image
                if (IsExpired(cachedImage.CreationTimeUtc) || await IsUpdatedAsync(cachedImage.CreationTimeUtc))
                {
                    CacheIndexer.Remove(CachedPath);
                    isUpdated = true;
                }
            }

            return isUpdated;
        }

        /// <summary>
        /// Adds the image to the cache in an asynchronous manner.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the image data.
        /// </param>
        /// <param name="contentType">
        /// The content type of the image.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override async Task AddImageToCacheAsync(Stream stream, string contentType)
        {
            CloudBlockBlob blockBlob = rootContainer.GetBlockBlobReference(blobPath);

            await blockBlob.UploadFromStreamAsync(stream);

            blockBlob.Properties.ContentType = contentType;
            blockBlob.Properties.CacheControl = $"public, max-age={BrowserMaxDays * 86400}";
            await blockBlob.SetPropertiesAsync();

            blockBlob.Metadata.Add("ImageProcessedBy", "ImageProcessor.Web.Episerver.Azure" + AssemblyVersion);
            await blockBlob.SetMetadataAsync();
            return;
        }

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override Task TrimCacheAsync()
        {
            if (!TrimCache)
            {
                return Task.FromResult(0);
            }

            ScheduleCacheTrimmer(async token =>
            {
                BlobContinuationToken continuationToken = null;
                List<IListBlobItem> results = new List<IListBlobItem>();

                // Loop through the all the files in a non blocking fashion.
                do
                {
                    BlobResultSegment response = await rootContainer.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.Metadata, 5000, continuationToken, null, null, token);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results);
                }
                while (token.IsCancellationRequested == false && continuationToken != null);

                // Now leap through and delete.
                foreach (
                    CloudBlockBlob blob in
                    results.Where((blobItem, type) => blobItem is CloudBlockBlob)
                           .Cast<CloudBlockBlob>()
                           .OrderBy(b => b.Properties.LastModified?.UtcDateTime ?? new DateTime()))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (blob.Properties.LastModified.HasValue && !IsExpired(blob.Properties.LastModified.Value.UtcDateTime))
                    {
                        continue;
                    }

                    if (!blob.Name.Contains(prefix))
                    {
                        continue;
                    }
                    // Remove from the cache and delete each CachedImage.
                    CacheIndexer.Remove(blob.Name);
                    await blob.DeleteAsync(token);
                }
            });

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns a value indicating whether the requested image has been updated.
        /// </summary>
        /// <param name="creationDate">The creation date.</param>
        /// <returns>The <see cref="bool"/></returns>
        private async Task<bool> IsUpdatedAsync(DateTime creationDate)
        {
            bool isUpdated = false;

            try
            {
                CloudBlockBlob blockBlob = rootContainer.GetBlockBlobReference(blobPath);

                if (await blockBlob.ExistsAsync())
                {
                    // Pull the latest info.
                    await blockBlob.FetchAttributesAsync();

                    if (blockBlob.Properties.LastModified.HasValue)
                    {
                        isUpdated = blockBlob.Properties.LastModified.Value.UtcDateTime > creationDate;
                    }
                }
                else
                {
                    // Try and get the headers for the file, this should allow cache busting for remote files.
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RequestPath);
                    request.Method = "HEAD";

                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        isUpdated = response.LastModified.ToUniversalTime() > creationDate;
                    }
                }
            }
            catch
            {
                isUpdated = false;
            }

            return isUpdated;
        }

        /// <summary>
        /// Rewrites the path to point to the cached image.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> encapsulating all information about the request.
        /// </param>
        public override void RewritePath(HttpContext context)
        {

            CloudBlockBlob blockBlob = rootContainer.GetBlockBlobReference(blobPath);
            string blobWithSAS = GetSaSForBlob(blockBlob, SharedAccessBlobPermissions.Read);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(blobWithSAS);

            if (streamCachedImage)
            {
                // Map headers to enable 304s to pass through
                if (context.Request.Headers["If-Modified-Since"] != null)
                {
                    TrySetIfModifiedSinceDate(context, request);
                }

                string[] mapRequestHeaders = { "Cache-Control", "If-None-Match" };
                foreach (string h in mapRequestHeaders)
                {
                    if (context.Request.Headers[h] != null)
                    {
                        request.Headers.Add(h, context.Request.Headers[h]);
                    }
                }

                // Write the blob storage directly to the stream
                request.Method = "GET";
                request.Timeout = timeout;

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    // A 304 is not an error
                    // It appears that some CDN's on Azure (Akamai) do not work properly when making head requests.
                    // They will return a response url and other headers but a 500 status code.
                    if (ex.Response != null && (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotModified
                        || ex.Response.ResponseUri.AbsoluteUri.Equals(CachedPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        response = (HttpWebResponse)ex.Response;
                    }
                    else
                    {
                        response?.Dispose();
                        logger.Error("Unable to stream cached path: " + CachedPath);
                        return;
                    }
                }

                Stream cachedStream = response.GetResponseStream();

                if (cachedStream != null)
                {
                    HttpResponse contextResponse = context.Response;

                    contextResponse.ClearHeaders();

                    // If streaming but not using a CDN the headers will be null.
                    // See https://github.com/JimBobSquarePants/ImageProcessor/pull/466
                    string etagHeader = response.Headers["ETag"];
                    if (!string.IsNullOrWhiteSpace(etagHeader))
                    {
                        contextResponse.Headers.Add("ETag", etagHeader);
                    }

                    string lastModifiedHeader = response.Headers["Last-Modified"];
                    if (!string.IsNullOrWhiteSpace(lastModifiedHeader))
                    {
                        contextResponse.Headers.Add("Last-Modified", lastModifiedHeader);
                    }

                    cachedStream.CopyTo(contextResponse.OutputStream); // Will be empty on 304s
                    EpiserverImageProcessingModule.SetHeaders(
                        context,
                        response.StatusCode == HttpStatusCode.NotModified ? null : response.ContentType,
                        null,
                        BrowserMaxDays,
                        response.StatusCode);
                }

                cachedStream?.Dispose();
                response.Dispose();
            }
            else
            {
                // Prevent redundant metadata request if paths match.
                //if (this.CachedPath == this.cachedRewritePath)
                //{
                //    ImageProcessingModule.AddCorsRequestHeaders(context);
                //    context.Response.Redirect(this.CachedPath, false);
                //    return;
                //}


                // Redirect the request to the blob URL
                request.Method = "HEAD";
                request.Timeout = timeout;

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    response.Dispose();
                    EpiserverImageProcessingModule.AddCorsRequestHeaders(context);
                    context.Response.Redirect(blobWithSAS, false);
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;

                    if (response != null)
                    {
                        HttpStatusCode responseCode = response.StatusCode;

                        // A 304 (NotModified) is not an error
                        // It appears that some CDN's on Azure (Akamai) do not work properly when making head requests.
                        // They will return a response url and other headers but a 500 status code.
                        if (responseCode == HttpStatusCode.NotModified || response.ResponseUri.AbsoluteUri.Equals(CachedPath, StringComparison.OrdinalIgnoreCase))
                        {
                            response.Dispose();
                            EpiserverImageProcessingModule.AddCorsRequestHeaders(context);
                            context.Response.Redirect(CachedPath, false);
                        }
                        else
                        {
                            response.Dispose();
                            logger.Error("Unable to rewrite cached path to: " + CachedPath);
                        }
                    }
                    else
                    {
                        // It's a 404, we should redirect to the cached path we have just saved to.
                        EpiserverImageProcessingModule.AddCorsRequestHeaders(context);
                        context.Response.Redirect(CachedPath, false);
                    }
                }
            }
        }


        /// <summary>
        /// Tries to set IfModifiedSince header however this crashes when context.Request.Headers["If-Modified-Since"] exists,
        /// but cannot be parsed. It cannot be parsed when it comes from Google Bot as UTC <example>Sun, 27 Nov 2016 20:01:45 UTC</example>
        /// so DateTime.TryParse. If it returns false, then log the error.
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="request">The current request</param>
        private static void TrySetIfModifiedSinceDate(HttpContext context, HttpWebRequest request)
        {

            string ifModifiedFromRequest = context.Request.Headers["If-Modified-Since"];

            if (DateTime.TryParse(ifModifiedFromRequest, out DateTime ifModifiedDate))
            {
                request.IfModifiedSince = ifModifiedDate;
            }
            else
            {
                if (ifModifiedFromRequest.ToLower().Contains("utc"))
                {
                    ifModifiedFromRequest = ifModifiedFromRequest.ToLower().Replace("utc", string.Empty);

                    if (DateTime.TryParse(ifModifiedFromRequest, out ifModifiedDate))
                    {
                        request.IfModifiedSince = ifModifiedDate;
                    }
                }
                else
                {
                    logger.Error($"Unable to parse date {context.Request.Headers["If-Modified-Since"]} for {context.Request.Url}");
                }
            }
        }

        /// <summary>
        /// Creates a SAS URI for the blob container.
        /// </summary>
        /// <param name="blobContainer"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        static string GetSaSForBlobContainer(CloudBlobContainer blobContainer, SharedAccessBlobPermissions permission)
        {
            var sas = blobContainer.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = permission,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),//SAS Start time is back by 5 minutes to take clock skewness into consideration
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
            });
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainer.Uri, sas);
        }

        /// <summary>
        /// Creates a SAS URI for the blob.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        static string GetSaSForBlob(CloudBlockBlob blob, SharedAccessBlobPermissions permission)
        {
            var sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = permission,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
            });
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sas);
        }
    }
}