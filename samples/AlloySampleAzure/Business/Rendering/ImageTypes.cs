using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageProcessor.Web.Episerver;

namespace AlloySampleAzure.Business.Rendering
{
    public static class ImageTypes
    {
        // A full width Hero image is very simple, since its always 100% of the viewport width.
        public static ImageType HeroImage = new ImageType
        {
            DefaultImgWidth = 1280,
            SrcSetWidths = new[] { 375, 750, 1440, 1920 },
            SrcSetSizes = new[] { "100vw" },
            HeightRatio = 0.5625 //16:9
        };

        public static ImageType Thumbnail = new ImageType
        {
            DefaultImgWidth = 200,
            SrcSetWidths = new[] { 200, 400 },
            SrcSetSizes = new[] { "200px" },
            HeightRatio = 1 //square
        };


        // A Teser image for the Episerver Alloy site.
        // Up to 980 pixels viewport width, the image "viewable width" will be 100% of the viewport - 40 pixels (margins).
        // Up to 1200 pixels viewport width, the image "viewable width" will be 298 pixels.
        // On larger viewport width, the image "viewable width" will be 368 pixels.
        // Note that the "viewable width" is not the same as the image file width (but it can be, on a screen with a "device pixel ratio" of 1). 
        public static ImageType Teaser = new ImageType
        {
            DefaultImgWidth = 750,
            SrcSetWidths = new[] { 298, 375, 500, 750, 980, 1024, 1400, 1500 }, //adding a bunch of sizes for demo purpose. In a real world scenario I wouldn't have this many.
            SrcSetSizes = new[] { "(max-width: 980px) calc((100vw - 40px)), (max-width: 1200px) 298px, 368px" }
        };
    }


}