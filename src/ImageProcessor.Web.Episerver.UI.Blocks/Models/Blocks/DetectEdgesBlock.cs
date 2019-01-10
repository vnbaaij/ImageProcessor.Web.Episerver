using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Detect Edges",
        GUID = "d4909010-f701-4c60-85ff-53789ca59743",
        Description = "Detects the edges in the current image using various one and two dimensional algorithms.",
        GroupName = Global.GroupName,
        Order = 9)]
    [Icon]
    public class DetectEdgesBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Filter")]
        [EnumAttribute(typeof(DetectEdgesFilter))]
        public virtual DetectEdgesFilter Filter { get; set; }

        public virtual bool Greyscale { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.DetectEdges(Filter, Greyscale);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Greyscale = false;
        }
    }
}