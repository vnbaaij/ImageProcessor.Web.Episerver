
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Caching;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.Services;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using EPiServer.Framework.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageProcessor.Web.Episerver.Azure
{
    /// <summary>
    /// Image service for retrieving images from Episerver running on Azure.
    /// </summary>
    public class ImageService : IImageService
    {
        private static string hostName = null;
        private static string containerName = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureImageService"/> class.
        /// </summary>
        public ImageService()
        {

            if (string.IsNullOrWhiteSpace(hostName))
            {
                string provider = EPiServerFrameworkSection.Instance.Blob.DefaultProvider;
                // Get the name of the connection string from configured provider
                string connectionStringName = EPiServerFrameworkSection.Instance.Blob.Providers[provider].Parameters["connectionStringName"];
                containerName = EPiServerFrameworkSection.Instance.Blob.Providers[provider].Parameters["container"];

                // Retrieve storage accounts from connection string.
                CloudStorageAccount cloudCachedStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);

                // Create the blob client.
                CloudBlobClient cloudBlobClient = cloudCachedStorageAccount.CreateCloudBlobClient();

                hostName = cloudBlobClient.BaseUri.ToString();
            }

            this.Settings = new Dictionary<string, string>
            {
                { "MaxBytes", "4194304" },
                { "Timeout", "30000" },
                { "Host", hostName },
                { "Container", containerName }
            };
        }

        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the image service requests files from
        /// the locally based file system.
        /// </summary>
        public bool IsFileLocalService => false;

        /// <summary>
        /// Gets or sets any additional settings required by the service.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the white list of <see cref="System.Uri"/>.
        /// </summary>
        public Uri[] WhiteList { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">
        /// The image path.
        /// </param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        public virtual bool IsValidRequest(string path)
        {
            return ImageHelpers.IsValidImageExtension(path);
        }

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">
        /// The value identifying the image to fetch.
        /// </param>
        /// <returns>
        /// The <see cref="System.Byte"/> array containing the image data.
        /// </returns>
        public virtual async Task<byte[]> GetImage(object id)
        {
            Uri baseUri = new Uri(hostName);

            var content = UrlResolver.Current.Route(new UrlBuilder((string)id));

            string relativeResourceUrl = "";

            if (content == null || !content.QueryDistinctAccess(AccessLevel.Read))
            {
                return null;
            }

            if (content is IBinaryStorable binary)
            {
                relativeResourceUrl = binary.BinaryData.ID.AbsolutePath;
            }

            if (!string.IsNullOrEmpty(containerName))
            {
                // TODO: Check me.
                containerName = $"{containerName.TrimEnd('/')}/";
                if (!relativeResourceUrl.StartsWith($"{containerName}/"))
                {
                    relativeResourceUrl = $"{containerName}{relativeResourceUrl.TrimStart('/')}";
                }
            }

            Uri uri = new Uri(baseUri, relativeResourceUrl);
            RemoteFile remoteFile = new RemoteFile(uri)
            {
                MaxDownloadSize = int.Parse(this.Settings["MaxBytes"]),
                TimeoutLength = int.Parse(this.Settings["Timeout"])
            };

            byte[] buffer;

            // Prevent response blocking.
            WebResponse webResponse = await remoteFile.GetWebResponseAsync().ConfigureAwait(false);

            using (MemoryStream memoryStream = MemoryStreamPool.Shared.GetStream())
            {
                using (WebResponse response = webResponse)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            responseStream.CopyTo(memoryStream);

                            // Reset the position of the stream to ensure we're reading the correct part.
                            memoryStream.Position = 0;

                            buffer = memoryStream.GetBuffer();
                        }
                        else
                        {
                            throw new HttpException((int)HttpStatusCode.NotFound, $"No image exists at {uri}");
                        }
                    }
                }
            }

            return buffer;
        }
    }
}