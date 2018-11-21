using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
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
    }
}