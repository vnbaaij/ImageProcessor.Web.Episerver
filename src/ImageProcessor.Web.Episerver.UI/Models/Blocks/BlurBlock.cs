using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using ImageProcessor.Web.Episerver.UI.Business;

namespace ImageProcessor.Web.Episerver.UI.Models.Blocks
{
    [ContentType(DisplayName = "Blur", 
        GUID = "eb7430c8-baf7-4268-912d-33c29c652242", 
        Description = "Uses a Gaussian kernel to blur the current image.", 
        GroupName = Global.GroupName, 
        Order = 5)]
    [Icon]
    public class BlurBlock : ImageProcessorMethodBaseBlock
    {
        public virtual int Kernelsize { get; set; }
        public virtual double Sigma { get; set; }
        public virtual int Threshold { get; set; }

        public override UrlBuilder GetMethod(UrlBuilder url)
        {
            return url.Blur(Kernelsize, Sigma, Threshold);
        }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Sigma = 1.4;
            Threshold = 0;
        }
    }
}