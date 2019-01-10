using System.Web.Mvc;
using EPiServer.Web.Mvc;

namespace ImageProcessor.Web.Episerver.UI.Blocks.Controllers
{
    public class ProcessImageBlockController : BlockController<Models.Blocks.ProcessImageBlock>
    {
        public override ActionResult Index(Models.Blocks.ProcessImageBlock currentContent)
        {
            return PartialView("~/modules/_protected/ImageProcessor.Web.Episerver.UI.Blocks/Views/ProcessImageBlock.cshtml", currentContent);
        }
    }
}
