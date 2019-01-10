using System;
using Newtonsoft.Json;

namespace ImageProcessor.Web.Episerver.UI.Crop.Core
{
    [Serializable]
    public class ImageReference : MediaReference
    {
        [JsonIgnore]
        public override MediaReferenceType MediaType => MediaReferenceType.Image;

        public Guid ContentReferenceId { get; set; }
        public string ContentLink { get; set; }
        public CropDetails CropDetails { get; set; }
    }
}