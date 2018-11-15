using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Episerver.Business;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(
        DisplayName = "Processed Image",
        GUID = "c493e717-5bee-4de7-8d8b-3ba40e012304",
        Description = "Manipulate image stored in Episerver with ImageProcessor ")]
    //[SiteImageUrl]
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
        public virtual Url ImageUrl
        {
            get
            {
                var url = this.GetPropertyValue(b => b.ImageUrl);

                return url == null || url.IsEmpty()
                           ? new Url(string.Empty)
                           : url;
            }
            set
            {
                this.SetPropertyValue(b => b.ImageUrl, value);
            }
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

    }
}