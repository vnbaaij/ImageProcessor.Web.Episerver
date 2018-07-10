using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using EPiServer;
using EPiServer.Azure.Blobs;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Configuration;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Caching;

namespace ImageProcessor.Web.Episerver
{
    public class BlobCache : ImageCacheBase
    {
        /// <summary>
        /// The create time of the cached image
        /// </summary>
        private DateTime cachedImageCreationTimeUtc = DateTime.MinValue;

        public bool IsFileBlobCache { get; set; } = true;

        /// <summary>
        /// The cached Uri of a blob.
        /// </summary>
        private Uri CachedUri { get; set; }
        public Uri CachedContainerUri { get; private set; }

        private readonly IBlobFactory factory;
        private readonly string provider;
        private const string scheme = "epi.fx.blob";
        private const string prefix = "3p!_";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBlobCache"/> class.
        /// </summary>
        /// <param name="requestPath">
        /// The request path for the image.
        /// </param>
        /// <param name="fullPath">
        /// The full path for the image.
        /// </param>
        /// <param name="querystring">
        /// The querystring containing instructions.
        /// </param>
        public BlobCache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            factory = ServiceLocator.Current.GetInstance<IBlobFactory>();
            var registry = ServiceLocator.Current.GetInstance<IBlobProviderRegistry>();

            provider = registry.DefaultProvider; 

        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            string cachedFilename = prefix + await CreateCachedFileNameAsync();

            var media = UrlResolver.Current.Route(new UrlBuilder(FullPath)) as MediaData;

            string container = media?.BinaryDataContainer?.Segments[1];
            if (container == null)
            {
                return true;
            }

            string id = media.BinaryData.ID.ToString();

            CachedContainerUri = new Uri($"{scheme}://{provider}/{container}");
            CachedUri = new Uri($"{scheme}://{provider}/{container}/{cachedFilename}");
            CachedPath = CachedUri.ToString();

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(id);

            if (cachedImage == null)
            {
                if (GetBlobCreationTime(out DateTime dateTime))
                {
                    cachedImage = new CachedImage
                    {
                        Key = Path.GetFileNameWithoutExtension(CachedPath),
                        Path = CachedPath,
                        CreationTimeUtc = dateTime
                    };

                    CacheIndexer.Add(cachedImage);
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
                if (IsExpired(cachedImage.CreationTimeUtc) || IsUpdated(cachedImage.CreationTimeUtc))
                {
                    CacheIndexer.Remove(CachedPath);
                    isUpdated = true;
                }
                else
                {
                    // Set cachedImageCreationTimeUtc so we can sender Last-Modified or ETag header when using Response.TransmitFile()
                    cachedImageCreationTimeUtc = cachedImage.CreationTimeUtc;
                }
            }

            return isUpdated;
        }

        private bool GetBlobCreationTime(out DateTime creationTimeUtc)
        {
           var blob = factory.GetBlob(CachedUri);
            if (blob != null)
            {
                DateTime dateTime = DateTime.MinValue;
                if (blob.GetType() == typeof(FileBlob))
                {
                    creationTimeUtc = File.GetCreationTimeUtc((blob as FileBlob).FilePath);
                    if (creationTimeUtc == DateTime.MinValue) return false;
                    return true;
                }
                if (blob.GetType() == typeof(AzureBlob))
                {
                    creationTimeUtc = (blob as AzureBlob).Properties.LastModified.Value.UtcDateTime;
                    if (creationTimeUtc == DateTime.MinValue) return false;
                    return true;
                }
            }
            creationTimeUtc = DateTime.MinValue;
            return false;
        }

        public override Task AddImageToCacheAsync(Stream stream, string contentType)
        {
            var blob = factory.CreateBlob(CachedContainerUri, Path.GetExtension(CachedPath));

            if (blob.GetType() == typeof(AzureBlob))
            {
                var azureBlob = blob as AzureBlob;
                azureBlob.Properties.ContentType = contentType;
                azureBlob.Properties.CacheControl = $"public, max-age={BrowserMaxDays * 86400}";

                //azureBlob.Metadata.Add("ImageProcessedBy", "ImageProcessor.Web.Episerver.Azure" + AssemblyVersion);
            }

            blob.Write(stream);

            return Task.CompletedTask;
        }

        public override void RewritePath(HttpContext context)
        {
            // The cached file is valid so just rewrite the path.
            var virtualCachedFilePath = "~/" + EPiServerFrameworkSection.Instance.AppData.BasePath + "/blobs/" + CachedUri.Segments[1] + CachedUri.Segments[2];

            context.RewritePath(virtualCachedFilePath, false);
            return;
            //throw new NotImplementedException();
        }

        public override Task TrimCacheAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns a value indicating whether the requested image has been updated.
        /// </summary>
        /// <param name="creationDate">The creation date.</param>
        /// <returns>The <see cref="bool"/></returns>
        private bool IsUpdated(DateTime creationDate)
        {
            bool isUpdated = false;

            try
            {
                GetBlobCreationTime(out DateTime dateTime);

                isUpdated = dateTime > creationDate;

            }
            catch
            {
                isUpdated = false;
            }

            return isUpdated;
        }
    }
}
