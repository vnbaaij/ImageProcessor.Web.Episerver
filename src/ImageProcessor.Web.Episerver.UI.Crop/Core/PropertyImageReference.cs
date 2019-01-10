using System;
using EPiServer.Framework.DataAnnotations;
using EPiServer.PlugIn;

namespace ImageProcessor.Web.Episerver.UI.Crop.Core
{
    [Serializable]
    [EditorHint("ImageReference")]
    [PropertyDefinitionTypePlugIn(DisplayName = "ImageReference")]
    public class PropertyImageReference : PropertyMediaReferenceBase<ImageReference>
    {
       
    }
}