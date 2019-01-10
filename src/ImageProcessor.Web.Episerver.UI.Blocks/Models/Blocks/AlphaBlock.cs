using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Alpha",
        GUID = "438f850d-42e9-4806-986b-c01b9321a1f2",
        Description = "Adjusts the alpha transparency of images.",
        GroupName = Global.GroupName,
        Order = 1)]
    [Icon]
    public class AlphaBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Percentage", Description = "The desired transparency percentage")]
        [Range(-99, 99)]
        public virtual int Percentage { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Alpha(Percentage);
        }
    }
}