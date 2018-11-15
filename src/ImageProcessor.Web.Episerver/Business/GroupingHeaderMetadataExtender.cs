using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;

namespace ImageProcessor.Web.Episerver.Business
{
    [ServiceConfiguration(IncludeServiceAccessor = false)]
    public class GroupingHeaderMetadataExtender : IMetadataExtender
    {
        private readonly LocalizationService _localizationService;

        public GroupingHeaderMetadataExtender(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            var contenTypeName = string.Empty;
            foreach (var property in metadata.Properties.Cast<ExtendedMetadata>())
            {
                var groupingHeader = property.Attributes.OfType<GroupingHeaderAttribute>().FirstOrDefault();
                if (groupingHeader != null)
                {
                    if (string.IsNullOrWhiteSpace(contenTypeName))
                    {
                        contenTypeName = ((IContentData)property.Parent.Model).GetOriginalType().Name;
                    }

                    var resourceKey = $"/contenttypes/{contenTypeName}/properties/{property.PropertyName}/groupingHeader";
                    var title = _localizationService.GetString(resourceKey, groupingHeader.Title);

                    property.EditorConfiguration["groupingHeader"] = new
                    {
                        title
                    };
                }
            }
        }
    }
}
