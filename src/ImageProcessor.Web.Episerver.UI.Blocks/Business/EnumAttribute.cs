using EPiServer.Shell.ObjectEditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Business
{
    public class EnumAttribute : SelectOneAttribute, IMetadataAware
    {
        public EnumAttribute(Type enumType)
        {
            EnumType = enumType;
        }

        public Type EnumType { get; set; }

        public new void OnMetadataCreated(ModelMetadata metadata)
        {
            var enumType = metadata.ModelType;

            SelectionFactoryType = typeof(EnumSelectionFactory<>).MakeGenericType(EnumType);

            base.OnMetadataCreated(metadata);
        }
    }
}