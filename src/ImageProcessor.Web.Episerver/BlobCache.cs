using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
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

        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            var blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();

            string cachedFileName = prefix + await CreateCachedFileNameAsync();

            var blob = UrlResolver.Current.Route(new UrlBuilder(FullPath)) as IBinaryStorable;

            string blobFolder = blob?.BinaryDataContainer?.Segments[1];

            if (blobFolder == null) return true;

            CachedPath = new UrlBuilder(string.Join("/", blobFolder, cachedFileName)).ToString();


            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(CachedPath);

            if (cachedImage == null)
            {
                if (blobFactory.GetBlob(new UrlBuilder(CachedPath).Uri))
                {
                    cachedImage = new CachedImage
                    {
                        Key = Path.GetFileNameWithoutExtension(CachedPath),
                        Path = CachedPath,
                        CreationTimeUtc = File.GetCreationTimeUtc(CachedPath)
                    };

                    CacheIndexer.Add(cachedImage);
                }

                //string blobPath = CachedPath.Substring(blobContainer.Uri.ToString().Length + 1);
                //CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobPath);

                //if (await blockBlob.ExistsAsync())
                //{
                //    // Pull the latest info.
                //    await blockBlob.FetchAttributesAsync();

                //    if (blockBlob.Properties.LastModified.HasValue)
                //    {
                //        cachedImage = new CachedImage
                //        {
                //            Key = Path.GetFileNameWithoutExtension(CachedPath),
                //            Path = CachedPath,
                //            CreationTimeUtc = blockBlob.Properties.LastModified.Value.UtcDateTime
                //        };

                //        CacheIndexer.Add(cachedImage);
                //    }
                //}
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
                else
                {
                    // Set cachedImageCreationTimeUtc so we can sender Last-Modified or ETag header when using Response.TransmitFile()
                    cachedImageCreationTimeUtc = cachedImage.CreationTimeUtc;
                }
            }

            return isUpdated;
        }

        public override Task AddImageToCacheAsync(Stream stream, string contentType)
        {
                    throw new NotImplementedException();
        }

        public override void RewritePath(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override Task TrimCacheAsync()
        {
            throw new NotImplementedException();
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
                //string blobPath = CachedPath.Substring(blobContainer.Uri.ToString().Length + 1);
                //CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobPath);

                //if (await blockBlob.ExistsAsync())
                //{
                //    // Pull the latest info.
                //    await blockBlob.FetchAttributesAsync();

                //    if (blockBlob.Properties.LastModified.HasValue)
                //    {
                //        isUpdated = blockBlob.Properties.LastModified.Value.UtcDateTime > creationDate;
                //    }
                //}
                //else
                //{
                //    // Try and get the headers for the file, this should allow cache busting for remote files.
                //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RequestPath);
                //    request.Method = "HEAD";

                //    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                //    {
                //        isUpdated = response.LastModified.ToUniversalTime() > creationDate;
                //    }
                //}
            }
            catch
            {
                isUpdated = false;
            }

            return isUpdated;
        }
    }
}
