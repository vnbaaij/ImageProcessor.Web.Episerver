using System;
using System.Collections.Specialized;
using EPiServer;
using ImageProcessor.Imaging;

namespace ImageProcessor.Web.Episerver
{
    public static class UrlBuilderExtensions
    {
        public static UrlBuilder Add(this UrlBuilder target, string key, string value)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add(key, value);

            return target;
        }

        public static UrlBuilder Add(this UrlBuilder target, NameValueCollection collection)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add(collection);

            return target;
        }

        /// <summary>
        ///  Adjusts the alpha transparency of images.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="percentage">Desired percentage value (without the ‘%’)</param>
        /// <returns></returns>
        public static UrlBuilder Alpha(this UrlBuilder target, int percentage)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("alpha", percentage.ToString());

            return target;
        }

        /// <summary>
        /// Defines whether gif images are processed to preserve animation or processed keeping the first frame only.
        /// </summary>
        /// <remarks>Defaults to animationprocessmode=all</remarks>
        /// <param name="target"></param>
        /// <param name="mode">All or First</param>
        /// <returns></returns>
        public static UrlBuilder AnimationProcessMode(this UrlBuilder target, AnimationProcessMode mode = Imaging.AnimationProcessMode.All)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("animationprocessmode", mode.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Performs auto-rotation to ensure that EXIF defined rotation is reflected in the final image.
        /// </summary>
        /// <remarks>If EXIF preservation is set to preserve metadata during processing this method will not alter the images rotation.</remarks>
        /// <param name="target"></param>
        /// <param name="rotate">Autorotate true or false</param>
        /// <returns></returns>
        public static UrlBuilder Autorotate(this UrlBuilder target, bool rotate)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("autorotate", rotate.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Changes the background color of the current image. This functionality is useful for adding a background when resizing image formats without an alpha channel.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Either a hex rgb coded color (no #) or a named color</param>
        /// <returns></returns>
        public static UrlBuilder BackgroundColor(this UrlBuilder target, string color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("bgcolor", color.ToLower());

            return target;
        }

        /// <summary>
        /// Changes the background color of the current image. This functionality is useful for adding a background when resizing image formats without an alpha channel.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Decimal red, green, lue and alpha values</param>
        /// <returns></returns>

        public static UrlBuilder BackgroundColor(this UrlBuilder target, int r, int g, int b, int a)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (r < 0 || g < 0 || b < 0 || a < 0 || r > 255 || g > 255 || b > 255 | a > 55)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("bgcolor", string.Join(",", r.ToString(), g.ToString(), b.ToString(), a.ToString()));

            return target;
        }

        /// <summary>
        /// Adjusts the contrast of images.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contrast">Desired percentage value (without the ‘%’)</param>
        /// <returns></returns>
        public static UrlBuilder Contrast(this UrlBuilder target, int contrast)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (contrast < 0 || contrast > 100)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("contrast", contrast.ToString());

            return target;
        }


        /// <summary>
        /// Crops the current image to the given location and size. There are two modes available:
        /// 1. Pixel based - Supply the upper-left coordinates and the new width/height.
        /// 2. Percentage based - Supply the left, top, right, and bottom percentages as a decimal between 0 and 100 to crop with an indicator to switch mode.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="x">Upper left x coordinate / left percentage</param>
        /// <param name="y">Upper left y coordinate / top percentage</param>
        /// <param name="width">New width / right percentage</param>
        /// <param name="heigth">New height / bottom percentage</param>
        /// <param name="mode">Percentage</param>
        /// <returns></returns>
        public static UrlBuilder Crop(this UrlBuilder target, int x, int y, int width, int heigth, CropMode mode = CropMode.Pixels)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("crop", string.Join(",", x.ToString(), y.ToString(), width.ToString(), heigth.ToString()));
            }
            if (mode == CropMode.Percentage)
                target.QueryCollection.Add("cropmode", mode.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Detects the edges in the current image using various one and two dimensional algorithms. 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter">Available filters are:
        /// kayyali, kirsch, aplacian3X3, laplacian5X5, laplacianffgaussian, prewitt, robertscross, scharr, sobel</param>
        /// <param name="greyscale">If the greyscale parameter is set to false the detected edges will maintain the pixel colors of the originl image.</param>
        /// <returns></returns>
        public static UrlBuilder DetectEdges(this UrlBuilder target, string filter, bool greyscale)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("detectedges", filter);
                target.QueryCollection.Add("greyscale", greyscale.ToString().ToLower());
            }

            return target;
        }

        /// <summary>
        /// Crops an image to the area of greatest entropy. This method works best with images containing large areas of a single color or similar colors around the edges.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="entropy">Entropy value</param>
        /// <returns></returns>
        public static UrlBuilder EntropyCrop(this UrlBuilder target, int entropy)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("entropycrop", entropy.ToString());

            return target;
        }

        /// <summary>
        /// Applies a filter to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter">Available filters are:
        /// blackwhite, comic, gotham, greyscale, hisatch, invert, lomograph, losatch, polaroid, sepia</param>
        /// <returns></returns>
        public static UrlBuilder Filter(this UrlBuilder target, string filter)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("filter", filter);

            return target;
        }

        /// <summary>
        /// Flips the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="direction">horizontal, vertical or both</param>
        /// <returns></returns>
        public static UrlBuilder Flip(this UrlBuilder target, string direction)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("flip", direction);

            return target;
        }

        /// <summary>
        /// Sets the output format of the current image to the given value.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="format">Core supported formats are:
        /// jpg, jpeg, bmp, gif, png, png8, tif, tiff</param>
        /// <returns></returns>
        public static UrlBuilder Format(this UrlBuilder target, string format)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("format", format);

            return target;
        }

        /// <summary>
        /// Uses a Gaussian kernel to blur the current image.
        /// Additional optional properties are also available
        /// </summary>
        /// <param name="target"></param>
        /// <param name="kernelsize">Desired kernel size</param>
        /// <param name="sigma">Defaults to 1.4</param>
        /// <param name="threshold">Defaults to 0</param>
        /// <returns></returns>
        public static UrlBuilder Blur(this UrlBuilder target, int kernelsize, double sigma = 1.4, int threshold = 0)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("blur", kernelsize.ToString());
                target.QueryCollection.Add("sigma", sigma.ToString());
                target.QueryCollection.Add("threshold", threshold.ToString());

            }
            return target;
        }


        /// <summary>
        /// Uses a Gaussian kernel to sharpen the current image.
        /// Additional optional properties are also available
        /// </summary>
        /// <param name="target"></param>
        /// <param name="kernelsize">Desired kernel size</param>
        /// <param name="sigma">Defaults to 1.4</param>
        /// <param name="threshold">Defaults to 0</param>
        /// <returns></returns>
        public static UrlBuilder Sharpen(this UrlBuilder target, int kernelsize, double sigma = 1.4, int threshold = 0)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("sharpen", kernelsize.ToString());
                target.QueryCollection.Add("sigma", sigma.ToString());
                target.QueryCollection.Add("threshold", threshold.ToString());

            }
            return target;
        }

        /// <summary>
        /// Alters the hue of the current image changing the overall color. 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="angle">The angle can be and value between 0 and 360 degrees.</param>
        /// <param name="rotate">Rotate.</param>
        /// <returns></returns>
        public static UrlBuilder Hue(this UrlBuilder target, int angle, bool rotate = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (angle < 0 || angle > 360)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("hue", angle.ToString());
                target.QueryCollection.Add("hue.rotate", rotate.ToString().ToLower());

            }

            return target;
        }

        public static UrlBuilder Width(this UrlBuilder target, int width)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("width", width.ToString());

            return target;
        }

        public static UrlBuilder Height(this UrlBuilder target, int height)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("height", height.ToString());

            return target;
        }
    }
}
