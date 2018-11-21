using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Models.Blocks
{
    [ContentType(DisplayName = "Gamma", 
        GUID = "4b0e0f6f-e098-4986-b35e-d13586f889b8", 
        Description = "Alters the gamma for the current image.", 
        GroupName = Global.GroupName, 
        Order = 14)]
    [Icon]
    public class GammaBlock : ImageProcessorMethodBaseBlock
    {
        [Display(Name = "Value")]
        public virtual double Gamma { get; set; }
    }
}