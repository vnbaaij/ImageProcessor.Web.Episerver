using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace ImageProcessor.Web.Episerver.Business
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Cms.Shell.InitializableModule))]
    public class PropertyDefaultValueInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            context.InitComplete += (sender, args) =>
            {
                var editorRegistry = ServiceLocator.Current.GetInstance<MetadataHandlerRegistry>();
                editorRegistry.RegisterMetadataHandler(typeof(ContentData),
                    new GroupingHeaderMetadataExtender(ServiceLocator.Current.GetInstance<LocalizationService>()), null,
                    EditorDescriptorBehavior.PlaceLast);
            };
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
