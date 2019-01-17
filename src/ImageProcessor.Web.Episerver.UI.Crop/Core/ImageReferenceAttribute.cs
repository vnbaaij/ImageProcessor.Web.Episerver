using System;

namespace ImageProcessor.Web.Episerver.UI.Crop.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ImageReferenceAttribute : Attribute
    {
        //public ImageReferenceAttribute(Type[] allowedTypes)
        //{
        //    AllowedTypes = allowedTypes;
        //}

        public double CropRatio { get; set; }
        public Type[] AllowedTypes { get; set; }

    }
}