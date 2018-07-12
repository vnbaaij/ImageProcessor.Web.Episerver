using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;

namespace AlloySample.Models.Blocks
{
    [ContentType(
        DisplayName = "ProcessedImageBlock", 
        GUID = "c493e717-5bee-4de7-8d8b-3ba40e012304", 
        Description = "Manipulate image stored in Episerver with ImageProcessor ")]
    [SiteImageUrl]

    public class ProcessedImageBlock : BlockData
    {
        /// <summary>
        /// Gets the image to process
        /// </summary>
        /// <remarks></remarks>
        [DefaultDragAndDropTarget]
        [UIHint(UIHint.Image)]
        public virtual Url Url
        {
            get
            {
                var url = this.GetPropertyValue(b => b.Url);

                return url == null || url.IsEmpty()
                           ? new Url(string.Empty)
                           : url;
            }
            set
            {
                this.SetPropertyValue(b => b.Url, value);
            }
        }
    }
}