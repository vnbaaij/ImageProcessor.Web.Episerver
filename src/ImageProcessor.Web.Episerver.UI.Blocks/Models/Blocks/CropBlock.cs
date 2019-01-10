using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Crop",
        GUID = "c26bc228-1215-4c40-a294-7dae69aa37e4",
        Description = "Crops the current image to the given location and size.",
        GroupName = Global.GroupName,
        Order = 8)]
    [Icon]
    public class CropBlock : ImageProcessorMethodBaseBlock
    {

        [EnumAttribute(typeof(CropMode))]
        public virtual CropMode Mode { get; set; }

        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }

        [Range(0,100)]
        public virtual int Left { get; set; }
        [Range(0, 100)]
        public virtual int Top { get; set; }
        [Range(0, 100)]
        public virtual int Right { get; set; }
        [Range(0, 100)]
        public virtual int Bottom { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            if (Mode == CropMode.Pixels)
                return url.Crop(X, Y, Width, Height, Mode);
            else
                return url.Crop(Left, Top, Right, Bottom, CropMode.Percentage);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Mode = CropMode.Pixels;
        }
    }
}