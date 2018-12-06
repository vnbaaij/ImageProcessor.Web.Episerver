using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Background", 
        GUID = "8895582f-97ce-42f3-be8b-fb41ef933867",
        Description = "Adds an image background to the current image.",
        GroupName = Global.GroupName,
        Order = 4)]
    [Icon]
    public class BackgroundBlock : ImageProcessorMethodBaseBlock
    {
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual int Opacity { get; set; }
        [Display(Name = "Background image")]
        public virtual string BackgroundImage { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Overlay(BackgroundImage, X, Y, Width, Height, Opacity);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Opacity = 100;
        }
    }
}