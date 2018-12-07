using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Episerver.Extensions.Picture;

namespace ImageProcessor.Web.Episerver.Picture
{
	public static class PictureUtils
	{
	    public static PictureData GetPictureData(ContentReference imageReference, ImageType imageType, bool includeLowQuality = false)
	    {
	        var urlBuilder = new UrlBuilder(ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(imageReference));
	        return GetPictureData(urlBuilder, imageType, includeLowQuality);
	    }

        public static PictureData GetPictureData(string imageUrl, ImageType imageType, bool includeLowQuality = false)
	    {
	        var urlBuilder = new UrlBuilder(imageUrl);
	        return GetPictureData(urlBuilder, imageType, includeLowQuality);
	    }

	    /// <summary>
	    /// Get the data necessary for rendering a Picture element.
	    /// </summary>
        public static PictureData GetPictureData(UrlBuilder imageUrl, ImageType imageType, bool includeLowQuality = false)
	    {
            var pData = new PictureData();
	        var currentFormat = PictureUtils.GetFormatFromExtension(imageUrl.Path);
	        if (imageType.SrcSetWidths != null)
	        {
	            pData.SrcSet = BuildSrcSet(imageUrl, imageType, currentFormat);
                pData.ImgSrc = BuildQueryString(imageUrl, imageType, imageType.DefaultImgWidth, currentFormat);
	            pData.SizesAttribute = string.Join(", ", imageType.SrcSetSizes);

	            if (includeLowQuality)
	            {
	                pData.SrcSetLowQuality = BuildSrcSet(imageUrl, imageType, currentFormat, true);
	                pData.ImgSrcLowQuality = BuildQueryString(imageUrl, imageType, imageType.DefaultImgWidth, currentFormat, 10);
	            }

                //if jpg, also add webp versions
                if (currentFormat == "jpg")
	            {
	                pData.SrcSetWebp = BuildSrcSet(imageUrl, imageType, "webp");
	                if (includeLowQuality)
	                {
	                    pData.SrcSetLowQualityWebp = BuildSrcSet(imageUrl, imageType, "webp", true);
                    }
	            }
	        }

	        return pData;
	    }

	    private static string BuildSrcSet(UrlBuilder imageUrl, ImageType imageType, string format, bool lowQuality = false)
	    {
	        var lowQualityValue = format == "webp" ? 1 : 10; //webp can have lower quality value
	        var lowQualityFormat = format == "png" ? "png8" : format; //low quality png will be 8-bit

            var srcset = string.Empty;
            foreach (var width in imageType.SrcSetWidths)
	        {
	            if (lowQuality)
	            {
	                srcset += BuildQueryString(imageUrl, imageType, width, lowQualityFormat, lowQualityValue) + " " + width + "w, ";
                }
                else
	            {
	                srcset += BuildQueryString(imageUrl, imageType, width, format) + " " + width + "w, ";
                }
            }
	        srcset = srcset.TrimEnd(',', ' ');

	        return srcset;
	    }

        private static string GetFormatFromExtension(string filePath)
		{
			var extension = Path.GetExtension(filePath);
			var format = extension?.TrimStart('.');
			if (format == "jpeg")
				format = "jpg";
			return format ?? string.Empty;
		}

		private static string BuildQueryString(UrlBuilder imageUrl, ImageType imageType, int? imageWidth, string format, int? overrideQuality = null)
		{
		    var currentQueryKeys = imageUrl.QueryCollection.AllKeys;
            var qc = new NameValueCollection();

			if (format == "webp" || format == "png8")
			{
				qc.Add("format", format);
			}

			if (format != "png" && format != "png8") //quality is ignored for png anyway
			{
                if (overrideQuality.HasValue)
                {
                    qc.Add("quality", overrideQuality.ToString());
                }
                else
                {
                    if (!currentQueryKeys.Contains("quality")) //don't change quality value if it already exists
                    {
                        qc.Add("quality", imageType.Quality.ToString());
                    }
                }
			}

			qc.Add("width", imageWidth.ToString());

			if (imageType.HeightRatio > 0)
			{
			    if (!currentQueryKeys.Contains("mode")) //don't change mode value if it already exists
                {
			        qc.Add("mode", "crop");
                }
				qc.Add("heightratio", imageType.HeightRatio.ToString(CultureInfo.InvariantCulture));
			}

		    bool.TryParse(ConfigurationManager.AppSettings["ImageProcessorUseFocalPoint"], out var checkFocalPoint);
		    if (checkFocalPoint)
		    {
		        qc.Add(BuildFocalPointCollection(imageUrl));
            }

            bool.TryParse(ConfigurationManager.AppSettings["ImageProcessorDebug"], out var showDebugInfo);
			if (showDebugInfo)
			{
				qc.Add(BuildInfoCollection(imageType, imageWidth, format));
			}

			//clone imageUrl and merge querystring values to the clone
			var newTarget = new UrlBuilder(imageUrl.ToString()); 
			newTarget.MergeQueryCollection(qc);

            //make sure "quality" is last in querystring. It has to be after "format".
		    var quality = newTarget.QueryCollection.Get("quality");
		    if (!string.IsNullOrEmpty(quality))
		    {
		        newTarget.QueryCollection.Remove("quality");
		        newTarget.QueryCollection.Add("quality", quality);
            }

            return (string)newTarget;
		}

	    private static NameValueCollection BuildFocalPointCollection(UrlBuilder imageUrl)
	    {
	        var queryCollection = new NameValueCollection();
	        var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
	        IContent image = urlResolver.Route(imageUrl);
	        if (image?.Property["ImageProcessorFocalPoint"]?.Value != null)
	        {
	            var propertyValue = image.Property["ImageProcessorFocalPoint"].ToString();
	            var focalValues = propertyValue.Split('|');
	            if (focalValues.Length == 2)
	            {
	                var x = focalValues[0];
	                var y = focalValues[1];
	                queryCollection.Add("center", y + "," + x);
                }
	        }

	        return queryCollection;
        }

        private static NameValueCollection BuildInfoCollection(ImageType imageType, int? imageWidth, string format)
		{
			var queryCollection = new NameValueCollection();

			if (string.IsNullOrEmpty(format))
			{
				format = "original";
			}

			var height = Convert.ToInt32(imageWidth * imageType.HeightRatio);
			var watermark = $"format:%20{format};%20width:%20{imageWidth};" + (height > 0 ? $"%20height:%20{height}" : "");
			var fontsize = imageWidth > 700 ? 35 : 17;
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
