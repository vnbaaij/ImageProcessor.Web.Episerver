using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Rounded Corners",
        GUID = "60b84172-12a5-4987-88c4-c285381d9ceb", 
        Description = "Adds rounded corners to the current image.",
        GroupName = Global.GroupName, 
        Order = 26)]
    [Icon]
    public class RoundedCornersBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Radius")]
        public virtual int RoundedCorners { get; set; }

        [Display(Name = "Top left")]
        public virtual bool Tl { get; set; }

        [Display(Name = "Top right")]
        public virtual bool Tr { get; set; }

        [Display(Name = "Bottom left")]
        public virtual bool Bl { get; set; }

        [Display(Name = "Bottom right")]
        public virtual bool Br { get; set; }
    }
}