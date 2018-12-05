namespace ImageProcessor.Web.Episerver
{
    public enum DetectEdgesFilter
    {
        Kayyali,
        Kirsch,
        Aplacian3X3,
        Laplacian5X5,
        Laplacianffgaussian,
        Prewitt,
        Robertscross,
        Scharr,
        Sobel
    }

    public enum Filter
    {
        Blackwhite,
        Comic,
        Gotham,
        Greyscale,
        Hisatch,
        Invert,
        Lomograph,
        Losatch,
        Polaroid,
        Sepia
    }

    public enum FlipDirection
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum ImageFormat
    {
        Jpg,
        Jpeg,
        Bmp,
        Gif,
        Png,
        Png8,
        Tif,
        Tiff
    }

    
}
