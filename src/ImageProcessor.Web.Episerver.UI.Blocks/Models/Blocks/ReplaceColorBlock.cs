using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Models.Blocks
{
    [ContentType(DisplayName = "Replace Color",
        GUID = "a9125ab3-c281-4a18-96c2-97fdb34a5733",
        Description = "Replaces one color with another within the current image.",
        GroupName = Global.GroupName,
        Order = 22)]
    [Icon]
    public class ReplaceColorBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "From")]
        [UIHint("ColorPicker")]
        public virtual string ColorFrom {
            get
            {
                var color = this.GetPropertyValue(b => b.ColorFrom);

                if (color == null)
                    return string.Empty;
                color = color.TrimStart('#');
                return color;
            }
            set
            {
                this.SetPropertyValue(b => b.ColorFrom, value.TrimStart('#'));
            }
        }

        [Display(Name = "To")]
        [UIHint("ColorPicker")]
        public virtual string ColorTo
        {
            get
            {
                var color = this.GetPropertyValue(b => b.ColorTo);

                if (color == null)
                    return string.Empty;
                color = color.TrimStart('#');
                return color;
            }
            set
            {
                this.SetPropertyValue(b => b.ColorTo, value.TrimStart('#'));
            }
        }

        public virtual int Fuziness { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.ReplaceColor(ColorFrom, ColorTo, Fuziness);
        }
    }
}