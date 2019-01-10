using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Halftone", 
        GUID = "0ea05674-3096-40fe-bf36-07b967b151b1", 
        Description = "Applies a classical CMYK halftone to the given image", 
        GroupName = Global.GroupName, 
        Order = 15)]
    [Icon]
    public class HalftoneBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Comic mode")]
        public virtual bool ComicMode { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Halftone(ComicMode);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            ComicMode = false;
        }
    }
}