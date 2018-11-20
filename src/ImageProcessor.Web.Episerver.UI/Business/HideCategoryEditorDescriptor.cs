using System;
using System.Collections.Generic;
using EPiServer.Cms.Shell.UI.ObjectEditing;
using EPiServer.Core;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using ImageProcessor.Web.Episerver.Models.Blocks;

namespace ImageProcessor.Web.Episerver.Business
{
    [EditorDescriptorRegistration(TargetType = typeof(CategoryList))]
    public class HideCategoryEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(
           ExtendedMetadata metadata,
           IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);

            var contentMetadata = (ContentDataMetadata)metadata;
            var ownerContent = contentMetadata.OwnerContent;
            if (ownerContent is ProcessImageBlock && metadata.PropertyName == "icategorizable_category")
            {
                metadata.ShowForEdit = false;
            }
        }
    }
}
