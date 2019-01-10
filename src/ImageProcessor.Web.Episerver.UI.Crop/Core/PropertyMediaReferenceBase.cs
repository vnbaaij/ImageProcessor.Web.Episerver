using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Logging;
using ImageProcessor.Web.Episerver.UI.Crop.Serialization;
using Newtonsoft.Json;

namespace ImageProcessor.Web.Episerver.UI.Crop.Core
{
    public abstract class PropertyMediaReferenceBase<T> : PropertyLongString
        where T : class

    {
        protected readonly ILogger _logger = LogManager.GetLogger();

        protected readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter> {new MediaConverter()},
            ContractResolver = new LowercaseContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public override Type PropertyValueType => typeof(T);

        public override object Value
        {
            get
            {
                var value = base.Value as string;
                if (value == null)
                    return null;

                return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
            }
            set
            {
                if (value != null)
                    base.Value = JsonConvert.SerializeObject(value, _serializerSettings);
            }
        }

        public override object SaveData(PropertyDataCollection properties)
        {
            return JsonConvert.SerializeObject(Value, _serializerSettings);
        }

        public override void LoadData(object value)
        {
            try
            {
                Value = JsonConvert.DeserializeObject((string) value, _serializerSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                Value = null;
            }
        }
    }
}