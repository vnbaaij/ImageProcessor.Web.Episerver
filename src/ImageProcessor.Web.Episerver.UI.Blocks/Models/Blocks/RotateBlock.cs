using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Rotate", 
        GUID = "bfb53865-6e3b-4f8a-ae1b-109d6990bcad", 
        Description = "Rotates the current image by the given angle without clipping.",
        GroupName = Global.GroupName,
        Order = 24)]
    [Icon]
    public class RotateBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Angle")]
        [Range(-360,360)]
        public virtual int Angle { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Rotate(Angle);
        }
    }
}