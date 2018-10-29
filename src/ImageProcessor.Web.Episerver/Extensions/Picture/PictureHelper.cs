using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using ImageProcessor.Web.Episerver.Picture;

namespace ImageProcessor.Web.Episerver
{
	public static class PictureHelper
    {
        public static IHtmlString Picture(this HtmlHelper helper, string imageUrl, ImageType imageType, string cssClass = "", LazyLoadType lazyLoadType = LazyLoadType.None)
        {
            if (imageUrl == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            var urlBuilder = new UrlBuilder(imageUrl);

            return Picture(helper, urlBuilder, imageType, cssClass, lazyLoadType);
        }

        public static IHtmlString Picture(this HtmlHelper helper, UrlBuilder imageUrl, ImageType imageType, string cssClass = "", LazyLoadType lazyLoadType = LazyLoadType.None)
        {
            if (imageUrl == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            //create picture element
            var pictureElement = new TagBuilder("picture");
	        var currentFormat = PictureUtils.GetFormatFromExtension(imageUrl.Path);
			if (imageType.SrcSetWidths != null)
            {
                //if jpg, also add webp source element
                if (currentFormat == "jpg")
                {
                    pictureElement.InnerHtml += BuildSourceElement(imageUrl, imageType, lazyLoadType, "webp");
                }

                //add source element to picture element
                pictureElement.InnerHtml += BuildSourceElement(imageUrl, imageType, lazyLoadType, currentFormat);
            }

            //add img element to picture element
	        pictureElement.InnerHtml += BuildImgElement(imageUrl, imageType, lazyLoadType, cssClass);

            return new MvcHtmlString(System.Web.HttpUtility.HtmlDecode(pictureElement.ToString()));
        }

	    private static string BuildImgElement(UrlBuilder imageUrlBuilder, ImageType imageType, LazyLoadType lazyLoadType, string cssClass)
	    {
			var imgElement = new TagBuilder("img");
		    imgElement.Attributes.Add("alt", "");

			//add src and/or data-src attribute
		    var imgSrc = PictureUtils.BuildQueryString(imageUrlBuilder, imageType, imageType.DefaultImgWidth);
		    switch (lazyLoadType)
		    {
			    case LazyLoadType.Regular:
				    imgElement.Attributes.Add("data-src", imgSrc);
				    break;
			    case LazyLoadType.Progressive:
					var imgSrcLowQuality = PictureUtils.BuildQueryString(imageUrlBuilder, imageType, imageType.DefaultImgWidth, overrideQuality: 10);
				    imgElement.Attributes.Add("src", imgSrcLowQuality);
					imgElement.Attributes.Add("data-src", imgSrc);
				    break;
			    default:
				    imgElement.Attributes.Add("src", imgSrc);
				    break;
		    }

			//add class attribute
		    if (!string.IsNullOrEmpty(cssClass))
		    {
			    imgElement.Attributes.Add("class", cssClass);
		    }

			return imgElement.ToString(TagRenderMode.SelfClosing);
		}

        private static string BuildSourceElement(UrlBuilder imageUrlBuilder, ImageType imageType, LazyLoadType lazyLoadType, string format = "")
        {
            var sourceElement = new TagBuilder("source");
	        var lowQualityValue = 10;

			if (format == "webp")
            {
                sourceElement.Attributes.Add("type", "image/" + format);
	            lowQualityValue = 1; //webp can have lower quality value 
			}

            //add srcset (and/or data-srcset) attribute
            var srcset = string.Empty;
	        var srcsetLowQuality = string.Empty;
	        var lowQualityFormat = format == "png" ? "png8" : format; //low quality png will be 8-bit
			foreach (var width in imageType.SrcSetWidths)
            {
                srcset += PictureUtils.BuildQueryString(imageUrlBuilder, imageType, width, format) + " " + width + "w, ";
	            if (lazyLoadType == LazyLoadType.Progressive)
	            {
		            srcsetLowQuality += PictureUtils.BuildQueryString(imageUrlBuilder, imageType, width, lowQualityFormat, lowQualityValue) + " " + width + "w, ";
	            }
			}
            srcset = srcset.TrimEnd(',', ' ');
	        srcsetLowQuality = srcsetLowQuality.TrimEnd(',', ' ');
	        switch (lazyLoadType)
	        {
		        case LazyLoadType.Regular:
			        sourceElement.Attributes.Add("data-srcset", srcset);
			        break;
		        case LazyLoadType.Progressive:
			        sourceElement.Attributes.Add("srcset", srcsetLowQuality);
			        sourceElement.Attributes.Add("data-srcset", srcset);
			        break;
		        default:
			        sourceElement.Attributes.Add("srcset", srcset);
			        break;
	        }

            //add sizes attribute
            sourceElement.Attributes.Add("sizes", string.Join(", ", imageType.SrcSetSizes));

            return sourceElement.ToString(TagRenderMode.SelfClosing);
        }
    }
}
