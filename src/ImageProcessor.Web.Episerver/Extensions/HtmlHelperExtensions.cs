using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Routing;



namespace ImageProcessor.Web.Episerver
{
    public static class HtmlHelperExtensions
    {
        public static UrlBuilder ProcessImage(this HtmlHelper helper, ContentReference image)
        {
            if (image == null || image == ContentReference.EmptyReference)
                throw new ArgumentNullException(nameof(image), "You might want to use `ProcessImageWithFallback()` instead");

            var url = UrlResolver.Current.GetUrl(image);

            return ConstructUrl(url);
        }

        public static UrlBuilder ProcessImageWithFallback(this HtmlHelper helper, ContentReference image, string imageFallback)
        {
            return ConstructUrl(image == null || image == ContentReference.EmptyReference ? imageFallback : UrlResolver.Current.GetUrl(image));
        }

        public static UrlBuilder ProcessImage(this HtmlHelper helper, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException(nameof(imageUrl), "You might want to use `ProcessImageWithFallback()` instead");

            return ConstructUrl(imageUrl);
        }

        public static UrlBuilder ProcessImageWithFallback(this HtmlHelper helper, string imageUrl, string imageFallback)
        {
            return ConstructUrl(string.IsNullOrEmpty(imageUrl) ? imageFallback : imageUrl);
        }

        private static UrlBuilder ConstructUrl(string url)
        {
            var builder = new UrlBuilder(url);

            return builder;
        }
    }
}


