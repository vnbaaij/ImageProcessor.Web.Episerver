using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Vignette", 
        GUID = "c2c4dea4-035c-4c18-a961-7bd3f46b0960", 
        Description = "Adds a vignette image effect to the current image.", 
        GroupName = Global.GroupName, 
        Order = 30)]
    [Icon]
    public class VignetteBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Color")]
        [UIHint("ColorPicker")]
        public virtual string Color
        {
            get
            {
                var color = this.GetPropertyValue(b => b.Color);

                if (color == null)
                    return string.Empty;
                color = color.TrimStart('#');
                return color;
            }
            set
            {
                this.SetPropertyValue(b => b.Color, value.TrimStart('#'));
            }
        }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Vignette(Color);
        }

        /// <summary>
        /// Sets the default property values on the content data.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Color = "000000";
        }
    }
}