using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Method Collection",
        GUID = "12fed908-e7b4-4dca-aee6-fcb65dcaf969",
        Description = "Re-useable grouping of image processing methods",
        GroupName = Global.GroupName,
        Order = 33)]
    [Icon]
    public class MethodCollectionBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Methods",
                Description = "Select the methods to process the image with",
                Order = 1)]
        [AllowedTypes(typeof(ImageProcessorMethodBaseBlock))]
        public virtual ContentArea Methods { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            if (Methods == null)
            {
                return url;
            }
            foreach (var item in Methods.FilteredItems)
            {
                if (item.GetContent() is ImageProcessorMethodBaseBlock method)
                {
                    method.GetMethod(url);
                }
            }
            return url;
        }
    }
}