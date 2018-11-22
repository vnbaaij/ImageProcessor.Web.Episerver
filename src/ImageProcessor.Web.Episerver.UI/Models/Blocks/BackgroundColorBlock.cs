using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Background Color",
        GUID = "4ddbb4bf-a836-4e4d-9cbf-fd2f792dbc53",
        Description = "Changes the background color of the current image.",
        GroupName = Global.GroupName,
        Order = 4)]
    [Icon]
    public class BackgroundColorBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Background color")]
        [UIHint("ColorPicker")]
        public virtual string Color {
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
            return url.BackgroundColor(Color);
        }
    }
}