using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using EPiServer;

namespace ImageProcessor.Web.Episerver
{
    public class ImageType
    {
        public int? DefaultImgWidth { get; set; } //this size will be used in browsers that don't support the picture element
        public int[] SrcSetWidths { get; set; } // the different image widths you want the browser to select from
        public string[] SrcSetSizes { get; set; }
        public double HeightRatio { get; set; }
        public int Quality { get; set; }

        public ImageType()
        {
            Quality = 80; //default quality
        }
    }

    public static class PictureHelper
    {
        public static IHtmlString Picture(this HtmlHelper helper, string imageUrl, ImageType imageType, string cssClass = "", bool lazyLoad = false)
        {
            if (imageUrl == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            var urlBuilder = new UrlBuilder(imageUrl);

            return Picture(helper, urlBuilder, imageType, cssClass, lazyLoad);
        }

        public static IHtmlString Picture(this HtmlHelper helper, UrlBuilder imageUrl, ImageType imageType, string cssClass = "", bool lazyLoad = false)
        {
            if (imageUrl == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            //create picture element
            var pictureElement = new TagBuilder("picture");

            if (imageType.SrcSetWidths != null)
            {
                //if jpg, also add webp source element
                if (imageUrl.Path.EndsWith(".jpg"))
                {
                    pictureElement.InnerHtml += BuildSourceElement(imageUrl, imageType, lazyLoad, "webp");
                }

                //add source element to picture element
                pictureElement.InnerHtml += BuildSourceElement(imageUrl, imageType, lazyLoad);
            }

            //create img element
            var imgElement = new TagBuilder("img");
	        imgElement.Attributes.Add("alt", "");
	        var attributeName = lazyLoad ? "data-src" : "src";
			imgElement.Attributes.Add(attributeName, BuildQueryString(imageUrl, imageType, imageType.DefaultImgWidth));
            if (!string.IsNullOrEmpty(cssClass))
            {
                imgElement.Attributes.Add("class", cssClass);
            }
            //add img element to picture element
            pictureElement.InnerHtml += imgElement.ToString(TagRenderMode.SelfClosing);

            return new MvcHtmlString(pictureElement.ToString());
        }

        private static string BuildSourceElement(UrlBuilder imageUrl, ImageType imageType, bool lazyLoad, string format = "")
        {
            var sourceElement = new TagBuilder("source");

            if (!string.IsNullOrEmpty(format))
            {
                //add type attribute
                sourceElement.Attributes.Add("type", "image/" + format);
            }

            //add srcset (or data-srcset) attribute
            var srcset = string.Empty;
            foreach (var width in imageType.SrcSetWidths)
            {
                srcset += BuildQueryString(imageUrl, imageType, width, format) + " " + width + "w, ";
            }
            srcset = srcset.TrimEnd(',', ' ');
	        var attributeName = lazyLoad ? "data-srcset" : "srcset";
	        sourceElement.Attributes.Add(attributeName, srcset);

            //add sizes attribute
            sourceElement.Attributes.Add("sizes", string.Join(", ", imageType.SrcSetSizes));

            return sourceElement.ToString(TagRenderMode.SelfClosing);
        }

        private static string BuildQueryString(UrlBuilder target, ImageType imageType, int? imageWidth, string format = "")
        {
            var qc = new NameValueCollection();

            if (!string.IsNullOrEmpty(format))
            {
                qc.Add("format", format); //format needs to be added before quality
            }

            qc.Add("quality", imageType.Quality.ToString());
            qc.Add("width", imageWidth.ToString());

            if (imageType.HeightRatio > 0)
            {
                qc.Add("mode", "crop");
                qc.Add("heightratio", imageType.HeightRatio.ToString());
            }

            bool.TryParse(ConfigurationManager.AppSettings["ImageProcessorDebug"], out var showDebugInfo);
            if (showDebugInfo)
            {
                qc.Add(BuildInfoCollection(imageType, imageWidth, format));
            }

            target.MergeQueryCollection(qc);

            return (string)target;
        }

        private static NameValueCollection BuildInfoCollection(ImageType imageType, int? imageWidth, string format)
        {
            var queryCollection = new NameValueCollection();

            if (string.IsNullOrEmpty(format))
            {
                format = "original";
            }

            var height = Convert.ToInt32(imageWidth * imageType.HeightRatio);
            string watermark = $"format:%20{format};%20width:%20{imageWidth};" + (height > 0 ? $"%20height:%20{height}" : "");
            var fontsize = imageWidth > 800 ? 35 : 17;
            var textX = imageWidth / 2 - 150;
            textX = textX < 0 ? 10 : textX;

            queryCollection.Add("watermark", watermark);
            queryCollection.Add("color", "000000");
            queryCollection.Add("fontsize", fontsize.ToString());
            queryCollection.Add("textposition", string.Join(",", textX.ToString(), Convert.ToInt32(height / 2).ToString()));

            return queryCollection;
        }
    }
}
