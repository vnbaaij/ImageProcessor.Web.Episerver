using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace ImageProcessor.Web.Episerver
{
    public static class PictureHelper
    {
        public static IHtmlString Picture(this HtmlHelper helper, string imageUrl, ImageType imageType, string cssClass = "")
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
                if (imageUrl.EndsWith(".jpg"))
                {
                    var webpSourceElement = GetSourceElement(imageUrl, imageType, "webp");
                    pictureElement.InnerHtml += webpSourceElement.ToString(TagRenderMode.SelfClosing);
                }

                //add source element to picture element
                var sourceElement = GetSourceElement(imageUrl, imageType);
                pictureElement.InnerHtml += sourceElement.ToString(TagRenderMode.SelfClosing);
            }

            //create img element
            var imgElement = new TagBuilder("img");
            imgElement.Attributes.Add("src", imageUrl + GetQueryString(imageType, imageType.DefaultImgWidth));
            if (!string.IsNullOrEmpty(cssClass))
            {
                imgElement.Attributes.Add("class", cssClass);
            }
            //add img element to picture element
            pictureElement.InnerHtml += imgElement.ToString(TagRenderMode.SelfClosing);

            return new MvcHtmlString(pictureElement.ToString());
        }

        private static TagBuilder GetSourceElement(string imageUrl, ImageType imageType, string format = "")
        {
            var sourceElement = new TagBuilder("source");

            if (!string.IsNullOrEmpty(format))
            {
                //add type attribute
                sourceElement.Attributes.Add("type", "image/" + format);
            }

            //add srcset attribute
            var srcset = string.Empty;
            foreach (var width in imageType.SrcSetWidths)
            {
                srcset += imageUrl + GetQueryString(imageType, width, format) + " " + width + "w, ";
            }
            sourceElement.Attributes.Add("srcset", srcset);

            //add sizes attribute
            var sizes = string.Empty;
            foreach (var size in imageType.SrcSetSizes)
            {
                sizes += size + ", ";
            }
            sourceElement.Attributes.Add("sizes", sizes);

            return sourceElement;
        }

        private static string GetQueryString(ImageType imageType, int? imageWidth, string format = "")
        {
            var qs = "?";

            if (!string.IsNullOrEmpty(format))
            {
                qs += $"format={format}&"; //format needs to be added before quality
            }

            qs += $"quality={imageType.Quality}&width={imageWidth}";

            if (imageType.HeightRatio > 0)
            {
                qs += $"&mode=crop&heightratio={imageType.HeightRatio}";
            }

            bool.TryParse(ConfigurationManager.AppSettings["ImageProcessorDebug"], out var showDebugInfo);
            if (showDebugInfo)
            {
                qs += GetInfoQueryString(imageType, imageWidth, format);
            }

            return qs;
        }

        private static string GetInfoQueryString(ImageType imageType, int? imageWidth, string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "origin";
            }
            var height = Convert.ToInt32(imageWidth * imageType.HeightRatio);
            var watermark = $"format:%20{format}%20width:%20{imageWidth}" + (height > 0 ? $"%20height:%20{height}" : "");
            var fontsize = imageWidth > 800 ? 35 : 17;
            var textX = imageWidth / 2 - 150;
            textX = textX < 0 ? 10 : textX;

            return $"&watermark={watermark}&color=FF2D31&fontsize={fontsize}&textposition={textX},{Convert.ToInt32(height / 2)}";
        }
    }

}
