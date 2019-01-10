using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Resize",
        GUID = "a51f38ca-eb17-4acd-a3d6-c688ca7f98a3",
        Description = "Resizes the current image to the given dimensions.",
        GroupName = Global.GroupName,
        Order = 23)]
    [Icon]
    public class ResizeBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Resize mode")]
        [EnumAttribute(typeof(ResizeMode))]
        public virtual ResizeMode Mode { get; set; }

        [Display(Name = "Anchor position")]
        [EnumAttribute(typeof(AnchorPosition))]
        public virtual AnchorPosition Anchor { get; set; }

        public virtual int Width { get; set; }
        public virtual int Height { get; set; }

        [Display(Name = "Width ratio")]
        public virtual double WidthRatio { get; set; }

        [Display(Name = "Height ratio")]
        public virtual double HeightRatio { get; set; }

        [Display(Name = "Center X")]
        public virtual double CenterX { get; set; }

        [Display(Name = "Center Y")]
        public virtual double CenterY { get; set; }

        [Display(Name = "Upscale")]
        public virtual bool Upscale { get; set; }


        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Resize(Width, Height, (float) WidthRatio, (float) HeightRatio, (float) CenterX, (float) CenterY, Mode, Anchor, Upscale);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Upscale = true;
            Anchor = AnchorPosition.Center;
            WidthRatio = 0;
            HeightRatio = 0;
            CenterX = 0;
            CenterY = 0;
            Mode = ResizeMode.Pad;
        }
    }
}