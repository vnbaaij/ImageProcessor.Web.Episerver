using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Halftone", 
        GUID = "0ea05674-3096-40fe-bf36-07b967b151b1", 
        Description = "Applies a classical CMYK halftone to the given image", 
        GroupName = Global.GroupName, 
        Order = 15)]
    [Icon]
    public class HalftoneBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Enable")]
        public virtual bool Halftone { get; set; }

        [Display(Name = "Comic mode")]
        public virtual bool ComicMode { get; set; }
    }
}