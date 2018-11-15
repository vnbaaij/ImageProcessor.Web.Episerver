using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace ImageProcessor.Web.Episerver.Business
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GroupingHeaderAttribute : Attribute
    {

        public string Title { get; set; }

        public GroupingHeaderAttribute(string title = "")
        {
            Title = title;
        }
    }
}