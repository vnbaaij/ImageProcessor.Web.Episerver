using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Flip", 
        GUID = "3e364fb8-b530-413b-818b-bd2a83b42616", 
        Description = "Flips the current image either horizontally, vertically, or both.", 
        GroupName = Global.GroupName, 
        Order = 12)]
    [Icon]
    public class FlipBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Flip direction")]
        [EnumAttribute(typeof(FlipDirection))]
        public virtual FlipDirection FlipDirection { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Flip(FlipDirection);
        }
    }
}