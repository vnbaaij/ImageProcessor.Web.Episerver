using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Auto Rotate", 
        GUID = "10afd0ec-2bab-4f1d-9a36-e6ee01576499", 
        Description = "Performs auto-rotation to ensure that EXIF defined rotation is reflected in the final image.", 
        GroupName = Global.GroupName, 
        Order = 3)]
    [Icon]
    public class AutoRotateBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Auto rotate", Description = "If EXIF preservation is set to preserve metadata during processing this method will not alter the images rotation.")]
        public virtual bool DoAutoRotate { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Autorotate(DoAutoRotate);
        }
    }
}