using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(
        DisplayName = "Process image",
        GUID = "c493e717-5bee-4de7-8d8b-3ba40e012304",
        Description = "Manipulate image stored in Episerver with ImageProcessor")]
    [Icon]
    public class ProcessImageBlock : ProcessImageBaseBlock
    {
        /// <summary>
        /// Gets the image to process
        /// </summary>
        /// <remarks></remarks>
        [DefaultDragAndDropTarget]
        [Display(Name = "Image", Order = 1)]
        [Required(AllowEmptyStrings = false)]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Image
        {
            get; set;
        }

        public UrlBuilder GetMethods()
        {
            var url = new UrlBuilder(UrlResolver.Current.GetUrl(Image));
            return MethodBuilder(url);
        }
    }
}