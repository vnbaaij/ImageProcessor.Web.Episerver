using System.ComponentModel.DataAnnotations;
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
        public virtual string BackgroundColor { get; set; }
    }
}