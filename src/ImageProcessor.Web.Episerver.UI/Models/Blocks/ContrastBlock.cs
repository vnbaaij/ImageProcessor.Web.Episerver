using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
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
        public virtual int Percentage { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Contrast(Percentage);
        }
    }
}