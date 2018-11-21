using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
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
        public virtual double RotateBounded { get; set; }

        [Display(Name = "Keep size")]
        public virtual bool KeepSize { get; set; }
    }
}