using EPiServer;
using EPiServer.Core;
using ImageProcessor.Web.Episerver.UI.Blocks.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    public abstract class ImageProcessorMethodBaseBlock : BlockData, IForceAllPropertiesMode
    {
        public abstract UrlBuilder GetMethod(UrlBuilder url);
    }
}
