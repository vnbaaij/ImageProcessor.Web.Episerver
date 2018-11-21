using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Vignette", 
        GUID = "c2c4dea4-035c-4c18-a961-7bd3f46b0960", 
        Description = "Adds a vignette image effect to the current image.", 
        GroupName = Global.GroupName, 
        Order = 30)]
    [Icon]
    public class VignetteBlock : ImageProcessorMethodBaseBlock
    {
        public virtual bool Enabled { get; set; }

        [Display(Name = "Color")]
        [UIHint("ColorPicker")]
        public virtual string Vignette { get; set; }

        /// <summary>
        /// Sets the default property values on the content data.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Enabled = false;
            Vignette = "#000000";

        }
    }
}