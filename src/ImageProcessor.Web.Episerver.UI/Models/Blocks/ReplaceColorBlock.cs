using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Replace Color",
        GUID = "a9125ab3-c281-4a18-96c2-97fdb34a5733",
        Description = "Replaces one color with another within the current image.",
        GroupName = Global.GroupName,
        Order = 22)]
    [Icon]
    public class ReplaceColorBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "From")]
        [UIHint("ColorPicker")]
        public virtual string ColorFrom { get; set; }

        [Display(Name = "To")]
        [UIHint("ColorPicker")]
        public virtual string ColorTo { get; set; }

        public virtual int Fuziness { get; set; }

    }
}