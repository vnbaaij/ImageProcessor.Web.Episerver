using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.Business;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Detect Edges", GUID = "d4909010-f701-4c60-85ff-53789ca59743", Description = "Detects the edges in the current image using various one and two dimensional algorithms.", GroupName = Global.GroupName)]
    public class DetectEdgesBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Filter")]
        [EnumAttribute(typeof(DetectEdgesFilter))]
        public virtual DetectEdgesFilter DetectEdges { get; set; }

        public virtual bool Greyscale { get; set; }
    }
}