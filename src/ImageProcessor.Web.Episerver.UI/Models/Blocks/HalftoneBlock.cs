using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Halftone", GUID = "0ea05674-3096-40fe-bf36-07b967b151b1", Description = "Applies a classical CMYK halftone to the given image", GroupName = Global.GroupName)]
    public class HalftoneBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Enable")]
        public virtual bool Halftone { get; set; }

        [Display(Name = "Comic mode")]
        public virtual bool ComicMode { get; set; }
    }
}