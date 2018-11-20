using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "RotateBoundedBlock", GUID = "0d536270-8fee-4413-86e9-42d7de84930f", Description = "Rotate the image without expanding the canvas.", GroupName = Global.GroupName)]
    public class RotateBoundedBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Rotate angle")]
        [Range(-360,360)]
        public virtual double RotateBounded { get; set; }

        [Display(Name = "Keep size")]
        public virtual bool KeepSize { get; set; }
    }
}