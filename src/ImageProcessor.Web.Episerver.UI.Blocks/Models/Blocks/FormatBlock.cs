using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Format", 
        GUID = "80b7d027-49c3-4d41-8b97-2e253eec30ce", 
        Description = "Sets the output format of the current image to the given value.", 
        GroupName = Global.GroupName, 
        Order = 13)]
    [Icon]
    public class FormatBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Image format")]
        [EnumAttribute(typeof(ImageFormat))]
        public virtual ImageFormat Format { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Format(Format);
        }
    }
}