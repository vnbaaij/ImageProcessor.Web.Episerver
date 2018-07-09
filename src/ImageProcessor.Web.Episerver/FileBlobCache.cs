using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Configuration;
using EPiServer.Logging;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Caching;

namespace ImageProcessor.Web.Episerver
{
    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses the configured Episerver Blob storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class FileBlobCache : ImageCacheBase
    {
        /// <summary>
        /// Use the configured logging mechanism
        /// </summary>
        private static readonly ILogger logger = LogManager.GetLogger();

        /// <summary>
        /// The virtual cache path.
        /// </summary>
        private readonly string virtualCachePath;

        /// <summary>
        /// The absolute path to virtual cache path on the server.
        /// </summary>
        private readonly string absoluteCachePath;

        /// <summary>
        /// The virtual path to the cached file.
        /// </summary>
        private string virtualCachedFilePath;

        /// <summary>
        /// The container where the blob is stored in.
        /// </summary>
        private string container;

        /// <summary>
        /// The cached generated filename
        /// </summary>
        private string cachedFilename;

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
        public FileBlobCache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            absoluteCachePath = GetAbsolutePath(out virtualCachePath);
        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            cachedFilename = prefix + await CreateCachedFileNameAsync();

            var media = UrlResolver.Current.Route(new UrlBuilder(FullPath)) as MediaData;

            container = media?.BinaryDataContainer?.Segments[1];
            if (container == null)
            {
                // We're working with a static file here
                container = $"_{prefix}static";
            }

            CachedPath = Path.Combine(absoluteCachePath, container, cachedFilename);
            virtualCachedFilePath = string.Join("/", virtualCachePath, container, cachedFilename);

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(cachedFilename);

            if (cachedImage == null)
            {
                if (File.Exists(CachedPath))
                {
                    cachedImage = new CachedImage
                    {
                        Key = Path.GetFileNameWithoutExtension(cachedFilename),
                        Path = CachedPath,
                        CreationTimeUtc = File.GetCreationTimeUtc(CachedPath)
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

        /// <summary>
        /// Adds the image to the cache in an asynchronous manner.
        /// </summary>
        /// <param name="stream">The stream containing the image data.</param>
        /// <param name="contentType">The content type of the image.</param>
        /// <returns>
        /// The <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override Task AddImageToCacheAsync(Stream stream, string contentType)
        {
            //DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(CachedPath));
            //if (!directoryInfo.Exists)
            //{
            //    directoryInfo.Create();
            //}

            //using (FileStream fileStream = File.Create(CachedPath))
            //{
            //    await stream.CopyToAsync(fileStream);
            //}

            var uri = new Uri(string.Format("{0}://{1}/{2}/{3}", Blob.BlobUriScheme, Blob.DefaultProvider, container, cachedFilename));
            FileBlob blob = new FileBlob(uri, CachedPath);

            blob.Write(stream);

            return Task.CompletedTask;
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

            ScheduleCacheTrimmer(token =>
            {
                string rootDirectory = Path.GetDirectoryName(CachedPath);

                if (rootDirectory != null)
                {
                    // Jump up to the parent branch to clean through the cache.
                    // UNC folders can throw exceptions if the file doesn't exist.
                    IEnumerable<string> directories = Directory.EnumerateDirectories(absoluteCachePath).Reverse();

                    foreach (string directory in directories)
                    {
                        if (!Directory.Exists(directory))
                        {
                            continue;
                        }

                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        IEnumerable<FileInfo> files = Directory.EnumerateFiles(directory)
                                                               .Select(f => new FileInfo(f))
                                                               .Where(f => f.Name.StartsWith(prefix))
                                                               .OrderBy(f => f.CreationTimeUtc);

                        foreach (FileInfo fileInfo in files)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            try
                            {
                                if (!IsExpired(fileInfo.CreationTimeUtc))
                                {
                                    continue;
                                }

                                // Remove from the cache and delete each CachedImage.
                                CacheIndexer.Remove(fileInfo.Name);
                                fileInfo.Delete();
                            }

                            catch (Exception ex)
                            {
                                // Log it but skip to the next file.
                                logger.Error($"Unable to clean cached file: {fileInfo.FullName}", ex);
                            }
                        }

                        // If the directory is empty of files delete it to remove the FCN.
                        RecursivelyDeleteEmptyDirectories(directory, absoluteCachePath, token);
                    }
                }
                return Task.FromResult(0);
            });

            return Task.FromResult(0);
        }

        /// <summary>
        /// Rewrites the path to point to the cached image.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> encapsulating all information about the request.
        /// </param>
        public override void RewritePath(HttpContext context)
        {
            context.RewritePath(virtualCachedFilePath, false);
        }

        private static string GetAbsolutePath(out string virtualPath)
        {
            virtualPath = "~/" + EPiServerFrameworkSection.Instance.AppData.BasePath + "/blobs";

            return HostingEnvironment.MapPath(virtualPath);
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
                if (new Uri(RequestPath).IsFile)
                {
                    if (File.Exists(RequestPath))
                    {
                        // If it's newer than the cached file then it must be an update.
                        isUpdated = File.GetLastWriteTimeUtc(RequestPath) > creationDate;
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
        /// Recursively delete the directories in the folder.
        /// </summary>
        /// <param name="directory">The current directory.</param>
        /// <param name="root">The root path.</param>
        /// <param name="token">The cancellation token.</param>
        private void RecursivelyDeleteEmptyDirectories(string directory, string root, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (directory == root)
                {
                    return;
                }

                // If the directory is empty of files delete it to remove the FCN.
                if (!Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Any() && !Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly).Any())
                {
                    Directory.Delete(directory);
                }

                RecursivelyDeleteEmptyDirectories(Directory.GetParent(directory).FullName, root, token);
            }
            catch (Exception ex)
            {
                // Log it but skip to the next directory.
                logger.Error($"Unable to clean cached directory: {directory}", ex);

            }
        }
    }
}