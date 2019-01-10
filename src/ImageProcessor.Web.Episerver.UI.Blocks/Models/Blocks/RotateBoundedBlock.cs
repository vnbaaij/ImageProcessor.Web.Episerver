using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Rotate Bounded Block", 
        GUID = "0d536270-8fee-4413-86e9-42d7de84930f",
        Description = "Rotate the image without expanding the canvas.", 
        GroupName = Global.GroupName, 
        Order = 25)]
    [Icon]
    public class RotateBoundedBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Rotate angle")]
        [Range(-360,360)]
        public virtual int Angle { get; set; }

        [Display(Name = "Keep size")]
        public virtual bool KeepSize { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.RotateBounded(Angle, KeepSize);
        }
    }
}