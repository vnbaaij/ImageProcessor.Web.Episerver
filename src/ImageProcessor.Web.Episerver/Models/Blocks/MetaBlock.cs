using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Meta", GUID = "e8270b25-2d7e-4abc-871d-35baf5a64016", Description = "Toggles preservation of EXIF defined metadata within the image.", GroupName = Global.GroupName)]
    public class MetaBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Preserve metadata")]
        public virtual bool Metadata { get; set; }
    }
}