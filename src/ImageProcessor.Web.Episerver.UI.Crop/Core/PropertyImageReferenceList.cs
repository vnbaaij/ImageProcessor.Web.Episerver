using System;
using EPiServer.Framework.DataAnnotations;
using EPiServer.PlugIn;
using ImageProcessor.Web.Episerver.UI.Crop.Core.Collections;

namespace ImageProcessor.Web.Episerver.UI.Crop.Core
{
    [Serializable]
    [EditorHint("ImageReferenceList")]
    [PropertyDefinitionTypePlugIn(DisplayName = "ImageReferenceList")]
    public class PropertyImageReferenceList : PropertyMediaReferenceBase<ImageReferenceList>
    {
    }
}