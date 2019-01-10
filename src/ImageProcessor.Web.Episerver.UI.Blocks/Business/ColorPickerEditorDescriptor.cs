using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using System;
using System.Collections.Generic;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Business.UIDescriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(string), UIHint = "ColorPicker")]
    public class ColorPickerEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            ClientEditingClass = "ip/editors/ColorPicker";
            base.ModifyMetadata(metadata, attributes);
        }
    }

}