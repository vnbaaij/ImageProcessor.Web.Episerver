using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageProcessor.Web.Episerver;

namespace AlloySample.Business.Rendering
{
    public static class ImageTypes
    {
        // A full width Hero image is very simple, since its always 100% of the viewport width.
        public static ImageType HeroImage = new ImageType
        {
            SrcSetWidths = new[] { 375, 750, 1440, 1920 },
            SrcSetSizes = new[] { "100vw" },
            HeightRatio = 0.5625 //16:9
        };

        public static ImageType Thumbnail = new ImageType
        {
            SrcSetWidths = new[] { 200, 400 },
            SrcSetSizes = new[] { "200px" },
            HeightRatio = 1 //square
        };


        // A Teaser image for the Episerver Alloy site.
        // Up to 980 pixels viewport width, the image "viewable width" will be 100% of the viewport - 40 pixels (margins).
        // Up to 1200 pixels viewport width, the image "viewable width" will be 368 pixels.
        // On larger viewport width, the image "viewable width" will be 750 pixels.
        // Note that the "viewable width" is not the same as the image file width (but it can be, on a screen with a "device pixel ratio" of 1).
        public static ImageType Teaser = new ImageType
        {
            SrcSetWidths = new[] { 375, 750, 980, 1500 }, //adding a bunch of sizes for demo purpose. In a real world scenario I wouldn't have this many.
            SrcSetSizes = new[] { "(max-width: 980px) calc((100vw - 40px)), (max-width: 1200px) 368px, 750px" },
	        HeightRatio = 0.5625 //16:9
		};
    }


}