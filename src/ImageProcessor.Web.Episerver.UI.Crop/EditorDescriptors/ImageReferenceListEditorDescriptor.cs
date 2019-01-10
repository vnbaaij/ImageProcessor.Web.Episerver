using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using ImageProcessor.Web.Episerver.UI.Crop.Core.Collections;

namespace ImageProcessor.Web.Episerver.UI.Crop.EditorDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(ImageReferenceList), UIHint = "ImageReferenceList")]
    public class ImageReferenceListEditorDescriptor : ImageReferenceBaseEditorDescriptor
    {
        public ImageReferenceListEditorDescriptor()
        {
            ClientEditingClass = "ipepiuicrop/Editors/ImageReferenceListSelector";
        }
    }
}