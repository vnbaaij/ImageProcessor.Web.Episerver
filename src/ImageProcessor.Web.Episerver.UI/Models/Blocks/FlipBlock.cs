using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.Business;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Flip", GUID = "3e364fb8-b530-413b-818b-bd2a83b42616", Description = "Flips the current image either horizontally, vertically, or both.", GroupName = Global.GroupName)]
    public class FlipBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Flip direction")]
        [EnumAttribute(typeof(FlipDirection))]
        public virtual FlipDirection Flip { get; set; }
    }
}