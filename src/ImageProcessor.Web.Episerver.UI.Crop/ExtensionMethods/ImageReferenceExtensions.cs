using System;
using EPiServer;
using EPiServer.Logging;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Episerver;
using ImageProcessor.Web.Episerver.UI.Crop.Core;

namespace ImageProcessor.Web.Episerver.UI.Crop.ExtensionMethods
{
    public static class ImageReferenceExtensions
    {
        private static readonly ILogger _logger;

        static ImageReferenceExtensions()
        {
            _logger = LogManager.GetLogger(typeof(ImageReferenceExtensions));
        }

        public static UrlBuilder GetCropUrl(this ImageReference imageReference, int? width = null, int? height = null,
            string fallback = null)
        {
            if (imageReference == null)
                return GetFallback(fallback);

            try
            {
                var url = UrlResolver.Current.GetUrl(new EPiServer.Core.ContentReference(imageReference.ContentLink));

                if (string.IsNullOrEmpty(url))
                    throw new Exception("Could not retrieve image's url");


                var urlBuilder = new UrlBuilder(url);

                if (imageReference.CropDetails != null)
                    urlBuilder.QueryCollection.Add("crop",
                        $"{imageReference.CropDetails.X},{imageReference.CropDetails.Y},{imageReference.CropDetails.X + imageReference.CropDetails.Width},{imageReference.CropDetails.Y + imageReference.CropDetails.Height}");

                if (width.HasValue && width > 0)
                    urlBuilder.Width(width.Value);

                if (height.HasValue && height > 0)
                    urlBuilder.Height(height.Value);

                return urlBuilder;
            }
            catch (Exception ex)
            {
                _logger.Error("GetCroupUrl failed", ex);
                return GetFallback(fallback);
            }
        }

        private static UrlBuilder GetFallback(string fallback)
        {
            if (string.IsNullOrEmpty(fallback))
                throw new ArgumentNullException("imageReference", "Image reference is null.");

            return new UrlBuilder(fallback);
        }
    }
}