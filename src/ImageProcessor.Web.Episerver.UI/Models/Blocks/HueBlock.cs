using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Hue", 
        GUID = "b0dd8d1e-1cb8-4bd2-81df-11ae90ffe8f2", 
        Description = "Alters the hue of the current image changing the overall color. ", 
        GroupName = Global.GroupName, 
        Order = 16)]
    [Icon]
    public class HueBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Angle")]
        [Range(0,360)]
        public virtual int Hue { get; set; }
        public virtual bool Rotate { get; set; }
    }
}