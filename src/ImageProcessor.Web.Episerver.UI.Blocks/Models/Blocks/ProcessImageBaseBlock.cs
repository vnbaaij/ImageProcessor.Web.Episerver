using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web.Routing;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    public class ProcessImageBaseBlock : BlockData
    {
        [Display(Name = "Methods",
               Description = "Select the methods to process the image with",
               Order = 2)]
        [AllowedTypes(typeof(ImageProcessorMethodBaseBlock))]
        public virtual ContentArea Methods { get; set; }

        [Display(Order = 3)]
        public virtual int? Width { get; set; }

        [Display(Order = 4)]
        public virtual int? Height { get; set; }

        public UrlBuilder MethodBuilder(UrlBuilder url)
        {

            if (Width > 0)
            {
                url.Width((int)Width);
            }
            if (Height > 0)
            {
                url.Height((int)Height);
            }

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
            return url;
        }
    }
}
