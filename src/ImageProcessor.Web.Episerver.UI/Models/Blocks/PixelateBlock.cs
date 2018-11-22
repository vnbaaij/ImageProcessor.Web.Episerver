using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Pixelate", 
        GUID = "9f61b2b0-0e12-46c5-95f8-4058adcb45b5", 
        Description = "Pixelates an image with the given size.",
        GroupName = Global.GroupName, 
        Order = 20)]
    [Icon]
    public class PixelateBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Pixelblock size")]
        public virtual int PixelBlockSize { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Pixelate(PixelBlockSize);
        }
    }
}