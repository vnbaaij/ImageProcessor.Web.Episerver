using System.Web;
using System.Web.Mvc;
using EPiServer;
using ImageProcessor.Web.Episerver.Extensions.Picture;
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

            var pictureData = PictureUtils.GetPictureData(imageUrl, imageType, includeLowQuality: lazyLoadType == LazyLoadType.Progressive);

            //Create picture element
            var pictureElement = new TagBuilder("picture");

			if (pictureData.SrcSet != null)
			{
			    if (pictureData.SrcSetWebp != null)
			    {
			        //Add source element with webp versions. Needs to be rendered before jpg version, browser selects the first version it supports.
			        pictureElement.InnerHtml += BuildSourceElement(pictureData, lazyLoadType, "webp");
			    }

                //Add source element to picture element
                pictureElement.InnerHtml += BuildSourceElement(pictureData, lazyLoadType);
            }

            //Add img element to picture element
            pictureElement.InnerHtml += BuildImgElement(pictureData, lazyLoadType, cssClass);

            return new MvcHtmlString(System.Web.HttpUtility.HtmlDecode(pictureElement.ToString()));
        }

	    private static string BuildImgElement(PictureData pictureData, LazyLoadType lazyLoadType, string cssClass)
	    {
			var imgElement = new TagBuilder("img");
		    imgElement.Attributes.Add("alt", "");

			//Add src and/or data-src attribute
		    switch (lazyLoadType)
		    {
			    case LazyLoadType.Regular:
				    imgElement.Attributes.Add("data-src", pictureData.ImgSrc);
				    break;
			    case LazyLoadType.Progressive:
				    imgElement.Attributes.Add("src", pictureData.ImgSrcLowQuality);
					imgElement.Attributes.Add("data-src", pictureData.ImgSrc);
				    break;
			    default:
				    imgElement.Attributes.Add("src", pictureData.ImgSrc);
				    break;
		    }

			//Add class attribute
		    if (!string.IsNullOrEmpty(cssClass))
		    {
			    imgElement.Attributes.Add("class", cssClass);
		    }

			return imgElement.ToString(TagRenderMode.SelfClosing);
		}

        private static string BuildSourceElement(PictureData pictureData, LazyLoadType lazyLoadType, string format = "")
        {
            var sourceElement = new TagBuilder("source");

            var srcset = pictureData.SrcSet;
			if (format == "webp")
            {
                srcset = pictureData.SrcSetWebp;
                sourceElement.Attributes.Add("type", "image/" + format);
            }

	        switch (lazyLoadType)
	        {
		        case LazyLoadType.Regular:
			        sourceElement.Attributes.Add("data-srcset", srcset);
			        break;
		        case LazyLoadType.Progressive:
			        sourceElement.Attributes.Add("srcset", pictureData.SrcSetLowQuality);
			        sourceElement.Attributes.Add("data-srcset", srcset);
			        break;
		        default:
			        sourceElement.Attributes.Add("srcset", srcset);
			        break;
	        }

            //Add sizes attribute
            sourceElement.Attributes.Add("sizes", pictureData.SizesAttribute);

            return sourceElement.ToString(TagRenderMode.SelfClosing);
        }
    }
}
