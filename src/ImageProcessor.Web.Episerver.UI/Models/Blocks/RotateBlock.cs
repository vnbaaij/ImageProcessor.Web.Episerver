using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Rotate", GUID = "bfb53865-6e3b-4f8a-ae1b-109d6990bcad", Description = "Rotates the current image by the given angle without clipping.", GroupName = Global.GroupName)]
    public class RotateBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Angle")]
        [Range(-360,360)]
        public virtual int Rotate { get; set; }
    }
}