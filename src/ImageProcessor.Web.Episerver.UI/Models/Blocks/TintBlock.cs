using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Tint", GUID = "6150367c-3f59-4e4d-b96b-68b0c6fc3a2d", Description = "Tints the current image with the given color.", GroupName = Global.GroupName)]
    public class TintBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Color")]
        [UIHint("ColorPicker")]
        public virtual string Tint { get; set; }
    }
}