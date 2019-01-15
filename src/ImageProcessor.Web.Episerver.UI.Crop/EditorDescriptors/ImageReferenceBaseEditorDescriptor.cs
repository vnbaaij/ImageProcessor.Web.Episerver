using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Shell;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using ImageProcessor.Web.Episerver.UI.Crop.Core;

namespace ImageProcessor.Web.Episerver.UI.Crop.EditorDescriptors
{
    public class ImageReferenceBaseEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);

            var mediaReferenceAttribute = attributes.OfType<ImageReferenceAttribute>().FirstOrDefault();

            if (mediaReferenceAttribute != null)
            {
                //metadata.EditorConfiguration["cropRatio"] = mediaReferenceAttribute.CropRatio;

                var allowedTypes = mediaReferenceAttribute.AllowedTypes;

                if (allowedTypes == null || !allowedTypes.Any())
                {
                    var imageDataType = typeof(ImageData);
                    allowedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                        .Where(t => imageDataType.IsAssignableFrom(t)).ToArray();
                }


                metadata.EditorConfiguration["allowedDndTypes"] =
                    allowedTypes.Select(x => x.FullName.ToLower()).ToArray();
            }

            metadata.CustomEditorSettings["uiWrapperType"] = UiWrapperType.Floating;
        }

        //public override void ModifyBaseMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        //{

        //}
    }
}