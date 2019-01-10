using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Brightness", 
        GUID = "290c6a81-2148-4716-bd68-024a3515bac8", 
        Description = "Adjusts the brightness of images.", 
        GroupName = Global.GroupName, 
        Order = 6)]
    [Icon]
    public class BrightnessBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Percentage", Description = "The desired adjustment percentage" )]
        [Range(-99, 99)]
        public virtual int Percentage { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Brightness(Percentage);
        }
    }
}