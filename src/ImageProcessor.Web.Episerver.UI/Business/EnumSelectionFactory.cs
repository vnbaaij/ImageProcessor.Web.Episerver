using System;
using System.Collections.Generic;
using EPiServer.Framework.Localization;
using EPiServer.Shell.ObjectEditing;

namespace ImageProcessor.Web.Episerver.Business
{
    public class EnumSelectionFactory<TEnum> : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(
            ExtendedMetadata metadata)
        {
            yield return new SelectItem
            {
                Text = "",
                Value = ""
            };
            var values = Enum.GetValues(typeof(TEnum));

            foreach (var value in values)
            {
                yield return new SelectItem
                {
                    Text = GetValueName(value),
                    Value = value
                };
            }
        }

        private string GetValueName(object value)
        {
            var staticName = Enum.GetName(typeof(TEnum), value);

            string localizationPath = string.Format(
                "/property/enum/{0}/{1}",
                typeof(TEnum).Name.ToLowerInvariant(),
                staticName.ToLowerInvariant());

            if (LocalizationService.Current.TryGetString(
                localizationPath,
                out string localizedName))
            {
                return localizedName;
            }

            return staticName;
        }
    }
}