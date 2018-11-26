using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(
        DisplayName = "Process Image",
        GUID = "c493e717-5bee-4de7-8d8b-3ba40e012304",
        Description = "Manipulate image stored in Episerver with ImageProcessor ")]
    [Icon]
    public class ProcessImageBlock : BlockData
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

        [Display(Name = "Methods",
                Description = "Select the methods to process the image with",
                Order = 2)]
        [AllowedTypes(typeof(ImageProcessorMethodBaseBlock))]
        public virtual ContentArea Methods { get; set; }

        [Display(Order = 3)]
        public virtual int Width { get; set; }

        [Display(Order = 4)]
        public virtual int Height { get; set; }

        public UrlBuilder GetMethods()
        {
            var url = new UrlBuilder(UrlResolver.Current.GetUrl(Image));
            return MethodBuilder(url);
        }

        private UrlBuilder MethodBuilder(UrlBuilder url)
        {
            if (Methods != null)
            {
                foreach (var item in Methods.FilteredItems)
                {
                    if (item.GetContent() is ImageProcessorMethodBaseBlock method)
                    {
                        method.GetMethod(url);
                    }
                }
            }
            if (Width > 0)
            {
                url.Width(Width);
            }
            if (Height > 0)
            {
                url.Height(Height);
            }
            return url;
        }

    }
}