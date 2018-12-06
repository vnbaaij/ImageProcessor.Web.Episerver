using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor.Web.Episerver.Extensions.Picture
{
    public class PictureData
    {
        public string SrcSet { get; set; }
        public string SrcSetWebp { get; set; }
        public string SrcSetLowQuality { get; set; }
        public string SrcSetLowQualityWebp { get; set; }
        public string SizesAttribute { get; set; }
        public string ImgSrc { get; set; }
        public string ImgSrcLowQuality { get; set; }

    }
}
