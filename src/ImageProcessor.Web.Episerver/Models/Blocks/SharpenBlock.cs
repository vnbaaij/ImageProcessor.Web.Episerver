using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Sharpen", GUID = "b47901f9-d24b-4d4e-a9f2-8b3e0caf8ebf", Description = "Uses a Gaussian kernel to sharpen the current image.", GroupName = Global.GroupName)]
    public class SharpenBlock : ImageProcessorMethodBaseBlock
    {
        public virtual int Kernelsize { get; set; }
        public virtual double Sigma { get; set; }
        public virtual int Threshold { get; set; }
    }
}