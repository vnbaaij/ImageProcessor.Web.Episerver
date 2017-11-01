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
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using ImageProcessor.Configuration;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Web.Caching;
using ImageProcessor.Web.Extensions;

namespace ImageProcessor.Web.Episerver
{
    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses the configured Episerver Blob storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class FileBlobCache : ImageCacheBase
    {

        /// <summary>
        /// Used to lock against when checking the cached folder path.
        /// </summary>
        private static object cachePathValidatorLock = new object();

        /// <summary>
        /// Use the configured logging mechanism
        /// </summary>
        private static readonly ILogger logger = LogManager.GetLogger();

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
        /// The create time of the cached image
        /// </summary>
        private DateTime cachedImageCreationTimeUtc = DateTime.MinValue;

        private Injected<IContentRepository> contentRepository;

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
            string configuredPath = "~/" + EPiServerFrameworkSection.Instance.AppData.BasePath + "/blobs";

            string virtualPath;
            this.absoluteCachePath = GetValidatedAbsolutePath(configuredPath, out virtualPath);
            this.virtualCachePath = virtualPath;
        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            // TODO: Before this check is performed it should be throttled. For example, only perform this check
            // if the last time it was checked is greater than 5 seconds. This would be much better for perf
            // if there is a high throughput of image requests.
            string cachedFileName = await this.CreateCachedFileNameAsync();

            var blob = UrlResolver.Current.Route(new UrlBuilder(this.FullPath)) as IBinaryStorable;
            string blobFolder = blob.BinaryDataContainer.Segments[1];


            this.CachedPath = Path.Combine(this.absoluteCachePath, blobFolder, cachedFileName);
            this.virtualCachedFilePath = string.Join("/",this.virtualCachePath, blobFolder, cachedFileName).Replace("//", "/");

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(this.CachedPath);

            if (cachedImage == null)
            {
                if (File.Exists(this.CachedPath))
                {
                    cachedImage = new CachedImage
                    {
                        Key = Path.GetFileNameWithoutExtension(this.CachedPath),
                        Path = this.CachedPath,
                        CreationTimeUtc = File.GetCreationTimeUtc(this.CachedPath)
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
                if (this.IsExpired(cachedImage.CreationTimeUtc) || await this.IsUpdatedAsync(cachedImage.CreationTimeUtc))
                {
                    CacheIndexer.Remove(this.CachedPath);
                    isUpdated = true;
                }
                else
                {
                    // Set cachedImageCreationTimeUtc so we can sender Last-Modified or ETag header when using Response.TransmitFile()
                    this.cachedImageCreationTimeUtc = cachedImage.CreationTimeUtc;
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
        public override async Task AddImageToCacheAsync(Stream stream, string contentType)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(this.CachedPath));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            using (FileStream fileStream = File.Create(this.CachedPath))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override Task TrimCacheAsync()
        {
            if (!this.TrimCache)
            {
                return Task.FromResult(0);
            }

            this.ScheduleCacheTrimmer(token =>
            {
                string rootDirectory = Path.GetDirectoryName(this.CachedPath);

                if (rootDirectory != null)
                {
                    // Jump up to the parent branch to clean through the cache.
                    // UNC folders can throw exceptions if the file doesn't exist.
                    IEnumerable<string> directories = SafeEnumerateDirectories(validatedAbsoluteCachePath).Reverse();

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
                                                               .OrderBy(f => f.CreationTimeUtc);
                                                               //.Skip(1);
                        foreach (FileInfo fileInfo in files)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            try
                            {
                                // If the group count is equal to the max count minus 1 then we know we
                                // have reduced the number of items below the maximum allowed.
                                // We'll cleanup any orphaned expired files though.
                                if (!this.IsExpired(fileInfo.CreationTimeUtc))
                                {
                                    break;
                                }
                                if (fileInfo.Name.Contains("_Thumbnail"))
                                {
                                    break;
                                }

                                if (!FileIsEpiserverBlob(fileInfo.Name, directory))
                                {
                                    // Remove from the cache and delete each CachedImage.
                                    CacheIndexer.Remove(fileInfo.Name);
                                    fileInfo.Delete();
                                }
                            }

                            catch (Exception ex)
                            {
                                // Log it but skip to the next file.
                                logger.Error($"Unable to clean cached file: {fileInfo.FullName}", ex);
                            }
                        }

                        // If the directory is empty of files delete it to remove the FCN.
                        this.RecursivelyDeleteEmptyDirectories(directory, validatedAbsoluteCachePath, token);
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
                context.RewritePath(this.virtualCachedFilePath, false);
            }
            else
            {
                // Check if the ETag matches (doing this here because context.RewritePath seems to handle it automatically
                string eTagFromHeader = context.Request.Headers["If-None-Match"];
                string eTag = this.GetETag();
                if (!string.IsNullOrEmpty(eTagFromHeader) && !string.IsNullOrEmpty(eTag) && eTagFromHeader == eTag)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    context.Response.StatusDescription = "Not Modified";
                    HttpModules.ImageProcessingModule.SetHeaders(context, this.BrowserMaxDays);
                    context.Response.End();
                }
                else
                {
                    // The file is outside of the web root so we cannot just rewrite the path since that won't work.
                    string extension = Helpers.ImageHelpers.Instance.GetExtension(this.FullPath, this.Querystring);
                    string mimeType = GetContentTypeForExtension(extension);
                    context.Response.ContentType = mimeType;

                    // Since we are going to call Response.End(), we need to go ahead and set the headers
                    HttpModules.ImageProcessingModule.SetHeaders(context, this.BrowserMaxDays);
                    this.SetETagHeader(context);
                    context.Response.AddHeader("Content-Length", new FileInfo(this.CachedPath).Length.ToString());

                    context.Response.TransmitFile(this.CachedPath);
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
                if (isInWebRoot)
                {
                    // If this is in the web root, we should just create it
                    Directory.CreateDirectory(dirInfo.FullName);
                }
                else
                {
                    throw new ConfigurationErrorsException("The cache folder " + absPath + " does not exist");
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
        private static string GetValidatedAbsolutePath(string originalPath, out string virtualPath)
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

                    string virtualCacheFolderPath;
                    string result = GetValidatedCachePathsImpl(
                        originalPath,
                        mapPath,
                        s => new DirectoryInfo(s),
                        out virtualCacheFolderPath);

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
        /// Returns an enumerable collection of directory paths that matches a specified search pattern and search subdirectory option.
        /// Will return an empty enumerable on exception. Quick and dirty but does what I need just now.
        /// </summary>
        /// <param name="directoryPath">
        /// The path to the directory to search within.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of directories. This parameter can contain a combination of valid literal path
        /// and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. The default pattern is "*", which returns all files.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specifies whether the search operation should include only
        /// the current directory or all subdirectories. The default value is AllDirectories.
        /// </param>
        /// <returns>
        /// An enumerable collection of directories that matches searchPattern and searchOption.
        /// </returns>
        private static IEnumerable<string> SafeEnumerateDirectories(string directoryPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            IEnumerable<string> directories;

            try
            {
                directories = Directory.EnumerateDirectories(directoryPath, searchPattern, searchOption);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }

            return directories;
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
                if (new Uri(this.RequestPath).IsFile)
                {
                    if (File.Exists(this.RequestPath))
                    {
                        // If it's newer than the cached file then it must be an update.
                        isUpdated = File.GetLastWriteTimeUtc(this.RequestPath) > creationDate;
                    }
                }
                else
                {
                    // Try and get the headers for the file, this should allow cache busting for remote files.
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.RequestPath);
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

                this.RecursivelyDeleteEmptyDirectories(Directory.GetParent(directory).FullName, root, token);
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
            string eTag = this.GetETag();
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
            if (this.cachedImageCreationTimeUtc != DateTime.MinValue)
            {
                long lastModFileTime = this.cachedImageCreationTimeUtc.ToFileTime();
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

        private bool FileIsEpiserverBlob(string path, string directory)
        {

            directory = Path.GetFileName(directory);

            var id = new Uri($"{Blob.BlobUriScheme}://{Blob.DefaultProvider}/{directory}/{path}");

            //var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var assetsRoot = SiteDefinition.Current.RootPage;

            var descendants = contentRepository.Service.GetDescendents(assetsRoot).Where(p => contentRepository.Service.Get<IContent>(p) is MediaData).Select(contentRepository.Service.Get<MediaData>);

            foreach (var image in descendants)
            {

                if (image.BinaryData.ID == id)
                {
                    return true;
                }

            }

            return false;
        }
    }
}