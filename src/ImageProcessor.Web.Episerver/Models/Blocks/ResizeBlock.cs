using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Episerver.Business;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Resize", GUID = "a51f38ca-eb17-4acd-a3d6-c688ca7f98a3", Description = "Resizes the current image to the given dimensions.", GroupName = Global.GroupName)]
    public class ResizeBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Resize mode")]
        public virtual ResizeMode Mode { get; set; }

        [Display(Name = "Anchor position")]
        [EnumAttribute(typeof(AnchorPosition))]
        public virtual AnchorPosition Anchor { get; set; }

        public virtual int Width { get; set; }
        public virtual int Height { get; set; }

        [Display(Name = "Width ratio")]
        public virtual double WidthRatio { get; set; }

        [Display(Name = "Height ratio")]
        public virtual double HeightRatio { get; set; }
    }
}