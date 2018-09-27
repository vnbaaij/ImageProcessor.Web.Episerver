using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;

namespace ImageProcessor.Web.Episerver.Picture
{
	internal static class PictureUtils
	{
		internal static string GetFormatFromExtension(string filePath)
		{
			var extension = Path.GetExtension(filePath);
			var format = extension?.TrimStart('.');
			if (format == "jpeg")
				format = "jpg";
			return format ?? string.Empty;
		}

		internal static string BuildQueryString(UrlBuilder imageUrlbuilder, ImageType imageType, int? imageWidth, string format = "", int? overrideQuality = null)
		{
			var qc = new NameValueCollection();

			if (format == "webp" || format == "png8")
			{
				qc.Add("format", format); //format needs to be added before quality
			}

			if (format != "png" &&  format != "png8") //quality is ignored for png anyway
			{
				var quality = overrideQuality ?? imageType.Quality;
				qc.Add("quality", quality.ToString());
			}

			qc.Add("width", imageWidth.ToString());

			if (imageType.HeightRatio > 0)
			{
				qc.Add("mode", "crop");
				qc.Add("heightratio", imageType.HeightRatio.ToString(CultureInfo.InvariantCulture));
			}

			bool.TryParse(ConfigurationManager.AppSettings["ImageProcessorDebug"], out var showDebugInfo);
			if (showDebugInfo)
			{
				qc.Add(BuildInfoCollection(imageType, imageWidth, format));
			}

			//clone imageUrlBuilder and merge querystring values to the clone
			var newTarget = new UrlBuilder(imageUrlbuilder.ToString()); 
			newTarget.MergeQueryCollection(qc);

			return (string)newTarget;
		}

		internal static NameValueCollection BuildInfoCollection(ImageType imageType, int? imageWidth, string format)
		{
			var queryCollection = new NameValueCollection();

			if (string.IsNullOrEmpty(format))
			{
				format = "original";
			}

			var height = Convert.ToInt32(imageWidth * imageType.HeightRatio);
			string watermark = $"format:%20{format};%20width:%20{imageWidth};" + (height > 0 ? $"%20height:%20{height}" : "");
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
