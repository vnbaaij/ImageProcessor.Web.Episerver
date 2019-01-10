using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
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
        public virtual int Radius { get; set; }

        [Display(Name = "Top left")]
        public virtual bool Tl { get; set; }

        [Display(Name = "Top right")]
        public virtual bool Tr { get; set; }

        [Display(Name = "Bottom left")]
        public virtual bool Bl { get; set; }

        [Display(Name = "Bottom right")]
        public virtual bool Br { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.RoundedCorners(Radius, Tl, Tr, Bl, Br);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Tl = true;
            Tr = true;
            Bl = true;
            Br = true;
        }
    }
}