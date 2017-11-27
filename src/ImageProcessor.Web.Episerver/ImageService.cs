using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.Services;
using EPiServer;

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
        public bool IsValidRequest(string path)
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
        public async Task<byte[]> GetImage(object id)
        {
            var content = UrlResolver.Current.Route(new UrlBuilder((string)id));

            if (content != null && content.QueryDistinctAccess(AccessLevel.Read))
            {
                if (content is IBinaryStorable binary)
                    return await Task.FromResult(binary.BinaryData.ReadAllBytes());
            }

            return null;
        }
    }
}