using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Overlay", 
        GUID = "554ba99c-7f42-40cb-a82b-39f0cdf1ba9b", 
        Description = "Adds a image overlay to the current image.", 
        GroupName = Global.GroupName,
        Order = 19)]
    [Icon]
    public class OverlayBlock : ImageProcessorMethodBaseBlock
    {
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual int Opacity { get; set; }
        [Display(Name = "Overlay image")]
        public virtual string Overlay { get; set; }
    }
}