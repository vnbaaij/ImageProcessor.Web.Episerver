using System;
using EPiServer.ServiceLocation;
using ImageProcessor.Web.Episerver.UI.Crop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageProcessor.Web.Episerver.UI.Crop.Serialization
{
    [ServiceConfiguration(typeof(JsonConverter))]
    public class MediaConverter : AbstractJsonConverter<MediaReference>
    {

        protected override MediaReference Create(Type objectType, JObject jObject)
        {
            if (FieldExists(jObject, "cropDetails", JTokenType.Object) || FieldExists(jObject, "CropDetails", JTokenType.Object))
                return new ImageReference();
            return null;
        }
    }
}