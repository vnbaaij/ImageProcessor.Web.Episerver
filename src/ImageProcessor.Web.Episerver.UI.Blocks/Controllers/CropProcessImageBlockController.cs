using System.Web.Mvc;
using EPiServer.Web.Mvc;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Controllers
{
    public class CropProcessImageBlockController : BlockController<Models.Blocks.CropProcessImageBlock>
    {
        public override ActionResult Index(Models.Blocks.CropProcessImageBlock currentContent)
        {
            return PartialView("~/modules/_protected/ImageProcessor.Web.Episerver.UI.Blocks/Views/CropProcessImageBlock.cshtml", currentContent);
        }
    }
}
