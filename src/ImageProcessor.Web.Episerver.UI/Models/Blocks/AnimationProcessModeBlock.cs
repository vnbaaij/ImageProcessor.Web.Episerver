using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Episerver.Business;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Animation Process Mode", GUID = "112157b5-2786-4173-918e-7e0c6bbf4417", Description = "Defines whether gif images are processed to preserve animation or processed keeping the first frame only.", GroupName = Global.GroupName)]
    public class AnimationProcessModeBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Animation Process Mode")]
        [EnumAttribute(typeof(AnimationProcessMode))]
        public virtual AnimationProcessMode AnimationProcessMode { get; set; }
    }
}