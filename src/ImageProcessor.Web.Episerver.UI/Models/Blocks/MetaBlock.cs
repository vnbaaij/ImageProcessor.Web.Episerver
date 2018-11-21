using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Meta", 
        GUID = "e8270b25-2d7e-4abc-871d-35baf5a64016", 
        Description = "Toggles preservation of EXIF defined metadata within the image.", 
        GroupName = Global.GroupName, 
        Order = 18)]
    [Icon]
    public class MetaBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Preserve metadata")]
        public virtual bool Metadata { get; set; }
    }
}