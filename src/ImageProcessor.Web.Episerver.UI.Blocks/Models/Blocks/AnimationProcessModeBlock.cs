using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Animation Process Mode", 
        GUID = "112157b5-2786-4173-918e-7e0c6bbf4417", 
        Description = "Defines whether gif images are processed to preserve animation or processed keeping the first frame only.", 
        GroupName = Global.GroupName, 
        Order = 2)]
    [Icon]
    public class AnimationProcessModeBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Animation Process Mode")]
        [EnumAttribute(typeof(AnimationProcessMode))]
        public virtual AnimationProcessMode Mode { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.AnimationProcessMode(Mode);
        }
    }
}