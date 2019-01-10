using System.ComponentModel.DataAnnotations;
using System.Drawing;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Watermark",
        GUID = "6f0efc40-7e9a-4d07-ade2-292630a59dbf",
        Description = "Adds a text based watermark to the current image with a wide range of options.",
        GroupName = Global.GroupName,
        Order = 31)]
    [Icon]
    public class WatermarkBlock : ImageProcessorMethodBaseBlock
    {
        private Point? position;

        [Display(Name = "Text")]
        public virtual string Text { get; set; }

        public virtual int X { get; set; }
        public virtual int Y { get; set; }

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

        [Display(Name = "Font family")]
        public virtual string FontFamily { get; set; }

        public virtual int Size { get; set; }

        [EnumAttribute(typeof(FontStyle))]
        public virtual FontStyle Style { get; set; }


        public virtual int Opacity { get; set; }
        public virtual bool Dropshadow { get; set; }
        public virtual bool Vertical { get; set; }
        public virtual bool Rtl { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            position = new Point(X, Y);
            return url.Watermark(Text, position, Color, FontFamily, Size, Style, Opacity, Dropshadow, Vertical, Rtl);
        }

        /// <summary>
        /// Sets the default property values on the content data.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Text = string.Empty;
            Color = "#ffffff";
            position = new Point(X, Y);
            FontFamily = null;
            Size = 48;
            Style = FontStyle.Bold;
            Opacity = 100;
            Dropshadow = false;
            Vertical = false;
            Rtl = false;
        }

    }

}