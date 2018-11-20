using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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
using ImageProcessor.Configuration;
using ImageProcessor.Imaging.Formats;
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
        /// Used to lock against when checking the cached folder path.
        /// </summary>
        private static object cachePathValidatorLock = new object();

        /// <summary>
        /// Whether the cached path has been checked.
        /// </summary>
        private static bool cachePathValidatorCheck;

        /// <summary>
        /// Stores the resulting validated absolute cache folder path
        /// </summary>
        private static string validatedAbsoluteCachePath;

        /// <summary>
        /// Stores the resulting validated virtual cache folder path - if it's within the web root
        /// </summary>
        private static string validatedVirtualCachePath;
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
            string basePath = (EPiServerFrameworkSection.Instance.AppData.BasePath.IndexOf("@\\") == 0)
                                        ? EPiServerFrameworkSection.Instance.AppData.BasePath
                                        : "~/" + EPiServerFrameworkSection.Instance.AppData.BasePath;

            string configuredPath = Settings.ContainsKey("VirtualCachePath")
                                        ? Settings["VirtualCachePath"]
                                        : basePath + "/blobs";

            absoluteCachePath = GetAbsolutePath(configuredPath, out virtualCachePath);
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
            if (!string.IsNullOrWhiteSpace(validatedVirtualCachePath))
            {
                // The cached file is valid so just rewrite the path.
                context.RewritePath(virtualCachedFilePath, false);
            }
            else
            {
                // Check if the ETag matches (doing this here because context.RewritePath seems to handle it automatically
                string eTagFromHeader = context.Request.Headers["If-None-Match"];
                string eTag = GetETag();
                if (!string.IsNullOrEmpty(eTagFromHeader) && !string.IsNullOrEmpty(eTag) && eTagFromHeader == eTag)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    context.Response.StatusDescription = "Not Modified";
                    HttpModules.ImageProcessingModule.SetHeaders(context, BrowserMaxDays);
                    context.Response.End();
                }
                else
                {
                    // The file is outside of the web root so we cannot just rewrite the path since that won't work.
                    string extension = Helpers.ImageHelpers.Instance.GetExtension(FullPath, Querystring);
                    string mimeType = GetContentTypeForExtension(extension);
                    context.Response.ContentType = mimeType;

                    // Since we are going to call Response.End(), we need to go ahead and set the headers
                    HttpModules.ImageProcessingModule.SetHeaders(context, BrowserMaxDays);
                    SetETagHeader(context);
                    context.Response.AddHeader("Content-Length", new FileInfo(CachedPath).Length.ToString());

                    context.Response.TransmitFile(CachedPath);
                    context.Response.End();
                }
            }
        }

        /// <summary>
        /// Returns the content-type/mime-type for a given image type based on it's file extension
        /// </summary>
        /// <param name="extension">
        /// Can be prefixed with '.' or not (i.e. ".jpg"  or "jpg")
        /// </param>
        /// <returns>The <see cref="string"/></returns>
        internal string GetContentTypeForExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(extension));
            }

            extension = extension.TrimStart('.');

            ISupportedImageFormat found = ImageProcessorBootstrapper.Instance.SupportedImageFormats
                .FirstOrDefault(x => x.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

            if (found != null)
            {
                return found.MimeType;
            }

            // default
            return new JpegFormat().MimeType;
        }

        /// <summary>
        /// The internal method that performs the actual validation which can be unit tested
        /// </summary>
        /// <param name="originalPath">
        /// The original path to validate which could be an absolute or a virtual path
        /// </param>
        /// <param name="mapPath">
        /// A function to use to perform the MapPath
        /// </param>
        /// <param name="getDirectoryInfo">
        /// A function to use to create the DirectoryInfo instance
        /// (this allows us to unit test)
        /// </param>
        /// <param name="virtualCachePath">
        /// If the absolute cache path is within the web root then the result of this will be the virtual path
        /// of the cache folder. If the absolute path is not within the web root then this will be null.
        /// </param>
        /// <returns>
        /// The absolute path to the cache folder
        /// </returns>
        internal static string GetValidatedCachePathsImpl(string originalPath, Func<string, string> mapPath, Func<string, FileSystemInfo> getDirectoryInfo, out string virtualCachePath)
        {
            string webRoot = mapPath("~/");

            string absPath = string.Empty;

            if (originalPath.IsValidVirtualPathName())
            {
                // GetFullPath will resolve any relative paths like ".." in the path
                absPath = Path.GetFullPath(mapPath(originalPath));
            }
            else if (Path.IsPathRooted(originalPath) && originalPath.IndexOfAny(Path.GetInvalidPathChars()) == -1)
            {
                // Determine if this is an absolute path
                // in this case this should be a real path, it's the best check we can do without a try/catch, but if this
                // does throw, we'll let it throw anyways.
                absPath = originalPath;
            }

            if (string.IsNullOrEmpty(absPath))
            {
                // Didn't pass the simple validation checks
                string message = "'VirtualCachePath' is not a valid virtual path. " + originalPath;
                logger.Critical(message);

                throw new ConfigurationErrorsException("FileBlobCache: " + message);
            }

            // Create a DirectoryInfo object to truly validate which will throw if it's not correct
            FileSystemInfo dirInfo = getDirectoryInfo(absPath);
            bool isInWebRoot = dirInfo.FullName.TrimEnd('/').StartsWith(webRoot.TrimEnd('/'));

            if (!dirInfo.Exists)
            {
                try
                {
                    // No matter if webroot or network UNC path - we need to create if it doesn't exist
                    Directory.CreateDirectory(dirInfo.FullName);
                }
                catch (Exception)
                {
                    throw new ConfigurationErrorsException("The cache folder " + absPath + " cannot be (re-)created");

                }
            }

            // This does a reverse map path:
            virtualCachePath = isInWebRoot
                                   ? dirInfo.FullName.Replace(webRoot, "~/").Replace(@"\", "/")
                                   : null;

            return dirInfo.FullName;
        }

        /// <summary>
        /// This will get the validated absolute path which is based on the configured value one time
        /// </summary>
        /// <param name="originalPath">The original path</param>
        /// <param name="virtualPath">The resulting virtual path if the path is within the web-root</param>
        /// <returns>The <see cref="string"/></returns>
        /// <remarks>
        /// We are performing this statically in order to avoid any overhead used when performing the validation since
        /// this occurs for each image when it only needs to be done once
        /// </remarks>
        private static string GetAbsolutePath(string originalPath, out string virtualPath)
        {
            string absoluteCachePath = LazyInitializer.EnsureInitialized(
                ref validatedAbsoluteCachePath,
                ref cachePathValidatorCheck,
                ref cachePathValidatorLock,
                () =>
                {
                    Func<string, string> mapPath = HostingEnvironment.MapPath;
                    if (originalPath.Contains("/.."))
                    {
                        // If that is the case this means that the user may be traversing beyond the wwwroot
                        // so we'll need to cater for that. HostingEnvironment.MapPath will throw a HttpException
                        // if the request goes beyond the webroot so we'll need to use our own MapPath method.
                        mapPath = s =>
                        {
                            try
                            {
                                return HostingEnvironment.MapPath(s);
                            }
                            catch (HttpException)
                            {
                                // need to user our own logic
                                return s.Replace("~/", HttpRuntime.AppDomainAppPath).Replace("/", "\\");
                            }
                        };
                    }

                    string result = GetValidatedCachePathsImpl(
                        originalPath,
                        mapPath,
                        s => new DirectoryInfo(s),
                        out string virtualCacheFolderPath);

                    validatedVirtualCachePath = virtualCacheFolderPath;
                    return result;
                });

            if (!string.IsNullOrWhiteSpace(validatedVirtualCachePath))
            {
                // Set the virtual cache path to the original one specified, it's just a normal virtual path like ~/App_Data/Blah
                virtualPath = validatedVirtualCachePath;
            }
            else
            {
                // It's outside of the web root, therefore it is an absolute path, we'll need to just have the virtualPath set
                // to the absolute path but deal with it accordingly based on the isCachePathInWebRoot flag
                virtualPath = absoluteCachePath;
            }

            return absoluteCachePath;
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

        /// <summary>
        /// Sets the ETag Header
        /// </summary>
        /// <param name="context"></param>
        private void SetETagHeader(HttpContext context)
        {
            string eTag = GetETag();
            if (!string.IsNullOrEmpty(eTag))
            {
                context.Response.Cache.SetETag(eTag);
            }
        }

        /// <summary>
        /// Creates an ETag value from the current creation time.
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        private string GetETag()
        {
            if (cachedImageCreationTimeUtc != DateTime.MinValue)
            {
                long lastModFileTime = cachedImageCreationTimeUtc.ToFileTime();
                DateTime utcNow = DateTime.UtcNow;
                long nowFileTime = utcNow.ToFileTime();
                string hexFileTime = lastModFileTime.ToString("X8", System.Globalization.CultureInfo.InvariantCulture);
                if ((nowFileTime - lastModFileTime) <= 30000000)
                {
                    return "W/\"" + hexFileTime + "\"";
                }

                return "\"" + hexFileTime + "\"";
            }
            return null;
        }
    }
}