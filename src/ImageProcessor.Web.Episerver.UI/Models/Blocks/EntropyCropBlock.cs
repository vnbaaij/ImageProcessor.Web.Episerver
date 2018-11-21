using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Entropy Crop", 
        GUID = "1a10e27d-83a9-4460-a743-306b074d9279", 
        Description = "Crops an image to the area of greatest entropy.", 
        GroupName = Global.GroupName, 
        Order = 10)]
    [Icon]
    public class EntropyCropBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Value")]
        public virtual int EntropyCrop { get; set; }
    }
}