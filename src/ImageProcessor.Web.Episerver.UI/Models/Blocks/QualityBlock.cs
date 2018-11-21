using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Quality", 
        GUID = "cc0e4ad4-d3ee-467e-877f-7cf7f4a50db8", 
        Description = "Alters the output quality of the current image.", 
        GroupName = Global.GroupName, 
        Order = 21)]
    [Icon]

    public class QualityBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Description = "Only effects the output quality of images that allow lossy processing.")]
        public virtual int Quality { get; set; }
    }
}