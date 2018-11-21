using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Saturation", 
        GUID = "2266f607-1fb9-4356-ba5b-d7a6a781311a", 
        Description = "Adjusts the saturation of images.", 
        GroupName = Global.GroupName, 
        Order = 27)]
    [Icon]
    public class SaturationBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Percentage", Description = "The desired adjustment percentage")]
        [Range(-99, 99)]
        public virtual int Saturation { get; set; }
    }
}