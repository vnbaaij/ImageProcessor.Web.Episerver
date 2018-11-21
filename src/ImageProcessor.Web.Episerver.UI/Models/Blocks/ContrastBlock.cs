using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Contrast", 
        GUID = "fdc63afb-d5c4-4124-a4b2-e79ba514f282", 
        Description = "Adjusts the brightness of images.", 
        GroupName = Global.GroupName, 
        Order = 7)]
    [Icon]
    public class ContrastBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Percentage", Description = "The desired adjustment percentage")]
        [Range(-99, 99)]
        public virtual int Contrast { get; set; }
    }
}