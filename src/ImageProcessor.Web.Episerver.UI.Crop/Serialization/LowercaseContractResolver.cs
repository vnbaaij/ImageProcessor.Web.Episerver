using System;
using Newtonsoft.Json.Serialization;

namespace ImageProcessor.Web.Episerver.UI.Crop.Serialization
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return Char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
        }
    }
}