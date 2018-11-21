using System.ComponentModel.DataAnnotations;
using System.Drawing;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using ImageProcessor.Web.Episerver.Business;

namespace ImageProcessor.Web.Episerver.Models.Blocks
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
        public virtual string Watermark { get; set; }

        public virtual int X { get; set; }
        public virtual int Y { get; set; }

        [UIHint("ColorPicker")]
        public virtual string Color { get; set; }

        public virtual string Font { get; set; }

        public virtual int Size { get; set; }

        [EnumAttribute(typeof(FontStyle))]
        public virtual FontStyle Style { get; set; }


        public virtual int Opacity { get; set; }
        public virtual bool Dropshadow { get; set; }
        public virtual bool Vertical { get; set; }
        public virtual bool Rtl { get; set; }

        /// <summary>
        /// Sets the default property values on the content data.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Watermark = string.Empty;
            Color = "white";
            position = new Point(X, Y);
            Font = null;
            Size = 48;
            Style = FontStyle.Bold;
            Opacity = 100;
            Dropshadow = false;
            Vertical = false;
            Rtl = false;
        }

    }

}