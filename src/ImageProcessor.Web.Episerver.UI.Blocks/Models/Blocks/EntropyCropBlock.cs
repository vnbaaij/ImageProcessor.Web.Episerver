using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
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
        public virtual int Entropy { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.EntropyCrop(Entropy);
        }
    }
}