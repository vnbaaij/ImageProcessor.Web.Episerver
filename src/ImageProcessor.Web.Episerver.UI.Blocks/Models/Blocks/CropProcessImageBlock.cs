using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;
using ImageProcessor.Web.Episerver.UI.Crop.Core;
using ImageProcessor.Web.Episerver.UI.Crop.ExtensionMethods;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(
        DisplayName = "Crop and process image",
        GUID = "4a4c93fd-4215-43a4-88aa-18b70cfa69f5",
        Description = "Crop and manipulate image stored in Episerver with ImageProcessor")]
    [Icon]
    public class CropProcessImageBlock : ProcessImageBaseBlock  
    {
        /// <summary>
        /// Gets the image to process
        /// </summary>
        /// <remarks></remarks>
        [ImageReference]
        [Display(Name = "Image", Order = 1)]
        [Required(AllowEmptyStrings = false)]
        public virtual ImageReference Image { get; set; }

        public UrlBuilder GetMethods()
        {
            var url = new UrlBuilder(Image.GetCropUrl());
            return MethodBuilder(url);
        }
    }
}