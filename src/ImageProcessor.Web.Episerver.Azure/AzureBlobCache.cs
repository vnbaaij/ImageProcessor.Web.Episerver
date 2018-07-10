using System;
using System.Collections.Generic;
using System.Configuration;
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

using ImageProcessor.Web.Caching;
using ImageProcessor.Web.HttpModules;

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
        private static readonly string AssemblyVersion = typeof(ImageProcessingModule).Assembly.GetName().Version.ToString();

        /// <summary>
        /// Use the configured logging mechanism
        /// </summary>
        private static readonly ILogger logger = LogManager.GetLogger();

        /// <summary>
        /// The cloud cached root blob container.
        /// </summary>
        private static CloudBlobContainer rootContainer;

        /// <summary>
        /// The timeout length for requesting the url.
        /// </summary>
        private readonly int timeout = 1000;

        public string blobPath;

        /// <summary>
        /// The provider name
        /// </summary>
        private static string providerName;

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
            providerName = EPiServerFrameworkSection.Instance.Blob.DefaultProvider;


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

                        CacheIndexer.Add(cachedImage);
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
            //context.RewritePath(CachedPath, false);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CachedPath);


            // Redirect the request to the blob URL
            request.Method = "HEAD";
            request.Timeout = timeout;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                response.Dispose();
                ImageProcessingModule.AddCorsRequestHeaders(context);
                context.Response.Redirect(CachedPath, false);
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
                        ImageProcessingModule.AddCorsRequestHeaders(context);
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
                    ImageProcessingModule.AddCorsRequestHeaders(context);
                    context.Response.Redirect(CachedPath, false);
                }
            }
        }
    }
}