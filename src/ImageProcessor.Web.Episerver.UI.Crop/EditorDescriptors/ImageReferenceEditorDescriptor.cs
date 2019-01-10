using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using ImageProcessor.Web.Episerver.UI.Crop.Core;

namespace ImageProcessor.Web.Episerver.UI.Crop.EditorDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(ImageReference), UIHint = "ImageReference")]
    public class ImageReferenceEditorDescriptor : ImageReferenceBaseEditorDescriptor
    {
        public ImageReferenceEditorDescriptor()
        {
            ClientEditingClass = "itmeric/Scripts/Editors/ImageReferenceSelector";
        }
    }
}