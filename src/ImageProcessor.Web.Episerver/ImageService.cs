using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.Services;


namespace ImageProcessor.Web.Episerver
{
    /// <summary>
    /// Image service for retrieving images from Episerver
    /// </summary>
    public class ImageService : IImageService
    {
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
            string image = id.ToString();
            byte[] buffer;

            // Check to see if the file exists.


            //return buffer;
            var content = UrlResolver.Current.Route(new UrlBuilder(image));

            if (content != null)
            {
                if (content.QueryDistinctAccess(AccessLevel.Read))
                    if (content is IBinaryStorable binary)
                        return await Task.FromResult(binary.BinaryData.ReadAllBytes());
            }
            else
            {
                // Map the request path if file local.
                Url url = new Url(image);
                var path = url.PathAndQuery;
                image = HostingEnvironment.MapPath(path);

                // Check to see if the file exists.
                if (!File.Exists(image))
                {
                    throw new HttpException((int)HttpStatusCode.NotFound, $"No image exists at {image}");
                }

                using (FileStream file = new FileStream(image, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    buffer = new byte[file.Length];
                    await file.ReadAsync(buffer, 0, (int)file.Length).ConfigureAwait(false);
                }

                return buffer;

            }
            return null;
        }
    }
}