using EPiServer.Configuration;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Business
{
    public class IconAttribute : ImageUrlAttribute
    {
        public IconAttribute() : base("~/ignored") { }

        public override string Path
        {
            get
            {
                var uiUrl = Settings.Instance.UIUrl.OriginalString.TrimStart("~/".ToCharArray()).TrimEnd("/".ToCharArray()).TrimEnd("CMS".ToCharArray());
                return "~/" + uiUrl + "/ImageProcessor.Web.Episerver.UI.Blocks/ipepi.png";
            }
        }
    }
}