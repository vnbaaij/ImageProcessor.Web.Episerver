using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Web.Mvc;
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
                target.QueryCollection.Add("animationprocessmode", mode.ToString().ToLowerInvariant());

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
                target.QueryCollection.Add("autorotate", rotate.ToString().ToLowerInvariant());

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
                target.QueryCollection.Add("bgcolor", color.ToLowerInvariant());

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
        [Obsolete("This method is deprecated, use DetectEdges method (with filter enum) instead", false)]
        public static UrlBuilder DetectEdges(this UrlBuilder target, string filter, bool greyscale)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("detectedges", filter);
                target.QueryCollection.Add("greyscale", greyscale.ToString().ToLowerInvariant());
            }

            return target;
        }

        /// <summary>
        /// Detects the edges in the current image using various one and two dimensional algorithms.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter">Available filters are defined in the <see cref="DetectEdgesFilter"/> enum</param>
        /// <param name="greyscale">If the greyscale parameter is set to false the detected edges will maintain the pixel colors of the originl image.</param>
        /// <returns></returns>
        public static UrlBuilder DetectEdges(this UrlBuilder target, DetectEdgesFilter filter, bool greyscale)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("detectedges", filter.ToString().ToLowerInvariant());
                target.QueryCollection.Add("greyscale", greyscale.ToString().ToLowerInvariant());
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
        [Obsolete("This method is deprecated, use Filter method (with filter enum) instead", false)]
        public static UrlBuilder Filter(this UrlBuilder target, string filter)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("filter", filter);

            return target;
        }

        /// <summary>
        /// Applies a filter to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter"><see cref="Filter"/></param>
        /// <returns></returns>
        public static UrlBuilder Filter(this UrlBuilder target, Filter filter)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("filter", filter.ToString().ToLowerInvariant());

            return target;
        }

        /// <summary>
        /// Flips the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="direction">horizontal, vertical or both</param>
        /// <returns></returns>
        [Obsolete("This method is deprecated, use Flip method (with FlipDirection enum) instead", false)]
        public static UrlBuilder Flip(this UrlBuilder target, string direction)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("flip", direction);

            return target;
        }

        /// <summary>
        /// Flips the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="direction"><see cref="FlipDirection"/></param>
        /// <returns></returns>
        public static UrlBuilder Flip(this UrlBuilder target, FlipDirection direction)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("flip", direction.ToString().ToLowerInvariant());

            return target;
        }

        /// <summary>
        /// Sets the output format of the current image to the given value.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="format">Core supported formats are:
        /// jpg, jpeg, bmp, gif, png, png8, tif, tiff</param>
        /// <returns></returns>
        [Obsolete("This method is deprecated, use Format method (with ImageFormat enum) instead")]
        public static UrlBuilder Format(this UrlBuilder target, string format)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("format", format);

            return target;
        }

        /// <summary>
        /// Sets the output format of the current image to the given value.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="format"><see cref="ImageFormat"/></param>
        /// <returns></returns>
        public static UrlBuilder Format(this UrlBuilder target, ImageFormat format)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("format", format.ToString().ToLowerInvariant());

            return target;
        }

        /// <summary>
        /// Change the alpha component of the image to effect its luminance
        /// </summary>
        /// <param name="target"></param>
        /// <param name="gamma">The gamma value</param>
        /// <returns></returns>
        public static UrlBuilder Gamma(this UrlBuilder target, float gamma)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("gamma", gamma.ToString());

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

            if (kernelsize < 0 || kernelsize > 22)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (sigma < 0 || sigma > 5.1)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (threshold < 0 || threshold > 100)
                throw new ArgumentOutOfRangeException(nameof(target));

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

            if (kernelsize < 0 || kernelsize > 22)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (sigma < 0 || sigma > 5.1)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (threshold < 0 || threshold > 100)
                throw new ArgumentOutOfRangeException(nameof(target));


            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("sharpen", kernelsize.ToString());
                target.QueryCollection.Add("sigma", sigma.ToString());
                target.QueryCollection.Add("threshold", threshold.ToString());

            }
            return target;
        }

        /// <summary>
        /// Applies halftone effect to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="comicMode">If true applies halftone with a comic book effect</param>
        /// <returns></returns>
        public static UrlBuilder Halftone(this UrlBuilder target, bool comicMode = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("halftone", !comicMode ? string.Empty : "comic");
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

        /// <summary>
        /// Applies the given image mask to the current image. Any area containing transparency withing the mask will be removed from the original image. If the mask is larger than the image it will be resized to match the images dimensions.
        /// The image mask is called by name alone.It has to be a locally stored file which defaults to the location ~/images/imageprocessor/mask/. This value is configurable.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mask">Mask image</param>
        /// <param name="x">Mask x position</param>
        /// <param name="y">Mask y position</param>
        /// <returns></returns>
        public static UrlBuilder Mask(this UrlBuilder target, string mask, int x, int y)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrWhiteSpace(mask))
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("mask", mask);
                target.QueryCollection.Add("mask.position", string.Join(",", x.ToString(), y.ToString()));

            }

            return target;
        }

        /// <summary>
        /// Toggles preservation of EXIF defined metadata within the image. Overrides the global variable set in the processing.config
        /// </summary>
        /// <param name="target"></param>
        /// <param name="preserve"></param>
        /// <returns></returns>
        public static UrlBuilder Meta(this UrlBuilder target, bool preserve)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("metadata", preserve.ToString().ToLower());
            }

            return target;
        }

        /// <summary>
        /// Adds a image overlay to the current image. If the overlay is larger than the image it will be resized to match the images dimensions.
        /// The image overlay is called by name alone.It has to be a locally stored file which defaults to the location ~/images/imageprocessor/overlay/. This value is configurable.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="overlay">Overlay image</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Heigth</param>
        /// <param name="opacity">Opacity</param>
        /// <returns></returns>
        public static UrlBuilder Overlay(this UrlBuilder target, string overlay, int x, int y, int width, int height, int opacity = 0)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrWhiteSpace(overlay))
                throw new ArgumentNullException(nameof(target));

            if (x < 0 || y < 0 || width <= 0 || height <= 0 || opacity < 0)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("overlay", overlay);
                target.QueryCollection.Add("overlay.position", string.Join(",", x.ToString(), y.ToString()));
                target.QueryCollection.Add("overlay.size", string.Join(",", width.ToString(), height.ToString()));
                target.QueryCollection.Add("overlay.opacity", opacity.ToString());

            }

            return target;
        }

        /// <summary>
        /// Pixelates an image with the given size.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="size">Size for pixels</param>
        /// <returns></returns>
        public static UrlBuilder Pixelate(this UrlBuilder target, int size)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("pixelate", size.ToString());
            }

            return target;
        }

        /// <summary>
        /// Alters the output quality of the current image. This method will only effect the output quality of images that allow lossy processing.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static UrlBuilder Quality(this UrlBuilder target, int quality)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (quality <= 0 || quality > 100)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("quality", quality.ToString());
            }

            return target;
        }

        /// <summary>
        /// Replaces a color within the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="colorFrom"></param>
        /// <param name="colorTo"></param>
        /// <param name="fuzziness"></param>
        /// <returns></returns>,
        // Not working with [r,g,b,a],[r,g,b,a]
        //public static UrlBuilder ReplaceColor(this UrlBuilder target, Color colorFrom, Color colorTo, int fuzziness)
        //{
        //    if (target == null)
        //        throw new ArgumentNullException(nameof(target));

        //    if (colorFrom == null || colorTo == null)
        //        throw new ArgumentOutOfRangeException(nameof(target));

        //    if (!target.IsEmpty)
        //    {
        //        target.QueryCollection.Add("replace", $"[{colorFrom.A.ToString()}, {colorFrom.R.ToString()}, {colorFrom.G.ToString()}, {colorTo.B.ToString()}],[{colorTo.A.ToString()},{colorTo.R.ToString()}, {colorTo.G.ToString()}, {colorTo.B.ToString()}]");
        //    }
        //    if (fuzziness > 0)
        //        target.QueryCollection.Add("fuzziness", fuzziness.ToString());
        //    return target;
        //}


        /// <summary>
        ///  Replaces a color within the current image
        /// </summary>
        /// <param name="target"></param>
        /// <param name="colorFrom">In KnownColor format</param>
        /// <param name="colorTo">In KnownColor format</param>
        /// <param name="fuzziness"></param>
        /// <returns></returns>
        public static UrlBuilder ReplaceColor(this UrlBuilder target, KnownColor colorFrom, KnownColor colorTo, int fuzziness)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("replace", $"{colorFrom.ToString().ToLower()},{colorTo.ToString().ToLower()}");
            }
            if (fuzziness > 0)
                target.QueryCollection.Add("fuzziness", fuzziness.ToString());
            return target;
        }

        /// <summary>
        ///  Replaces a color within the current image
        /// </summary>
        /// <param name="target"></param>
        /// <param name="colorFrom">In hex format. Needs UrlEncoded '#' if supplied, not required.</param>
        /// <param name="colorTo">In hex format. Needs UrlEncoded '#' if supplied, not required.</param>
        /// <param name="fuzziness"></param>
        /// <returns></returns>
        public static UrlBuilder ReplaceColor(this UrlBuilder target, string colorFrom, string colorTo, int fuzziness)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("replace", $"{colorFrom.ToLower()},{colorTo.ToLower()}");
            }
            if (fuzziness > 0)
                target.QueryCollection.Add("fuzziness", fuzziness.ToString());
            return target;
        }

        /// <summary>
        /// Resizes the current image to the given dimensions. If the set dimensions do not match the aspect ratio of the original image then the output is cropped to match the new aspect ratio.
        /// ImageProcessor.Web allows you to scale images both up and down with an excellent quality to size ratio.A maximum width and height can be set in the configuration to help protect you from DoS attacks.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width">New image width</param>
        /// <param name="height">New image height</param>
        /// <param name="centerX">A fraction from the top-left corner of the image (x-coordinate, between 0 and 1). This point will be as close to the center of your crop as possible while keeping the crop within the image.</param>
        /// <param name="centerY">A fraction from the top-left corner of the image (y-coordinate, between 0 and 1). This point will be as close to the center of your crop as possible while keeping the crop within the image<.</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="upscale">All image requests allow upscaling by default. A limiter is set in the processing.config file to help prevent DoS attacks.
        /// To turn off upscaling simply append the value upscale=false to your request url. (This does not affect stretched, boxpad, nor min modes). </param>
        /// <returns></returns>
        public static UrlBuilder ResizeCrop(this UrlBuilder target, int? width, int? height, float? centerX, float? centerY, AnchorPosition anchor = AnchorPosition.Center, bool upscale = true)
        {
            return Resize(target, width, height, null, null, centerX, centerY, ResizeMode.Crop, anchor, upscale);
        }

        /// <summary>
        /// Resizes the current image to the given dimensions. If the set dimensions do not match the aspect ratio of the original image then the output is cropped to match the new aspect ratio.
        /// ImageProcessor.Web allows you to scale images both up and down with an excellent quality to size ratio.A maximum width and height can be set in the configuration to help protect you from DoS attacks.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="widthratio">New width as a ratio</param>
        /// <param name="heightratio">New height as a ratio</param>
        /// <param name="centerX">A fraction from the top-left corner of the image (x-coordinate, between 0 and 1). This point will be as close to the center of your crop as possible while keeping the crop within the image.</param>
        /// <param name="centerY">A fraction from the top-left corner of the image (y-coordinate, between 0 and 1). This point will be as close to the center of your crop as possible while keeping the crop within the image<.</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="upscale">All image requests allow upscaling by default. A limiter is set in the processing.config file to help prevent DoS attacks.
        /// To turn off upscaling simply append the value upscale=false to your request url. (This does not affect stretched, boxpad, nor min modes). </param>
        /// <returns></returns>
        public static UrlBuilder ResizeCrop(this UrlBuilder target, float? widthratio, float? heightratio, float? centerX, float? centerY, AnchorPosition anchor = AnchorPosition.Center, bool upscale = true)
        {
            return Resize(target, null, null, widthratio, heightratio, centerX, centerY, ResizeMode.Crop, anchor, upscale);
        }

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// ImageProcessor.Web allows you to scale images both up and down with an excellent quality to size ratio.A maximum width and height can be set in the configuration to help protect you from DoS attacks.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width">New image width</param>
        /// <param name="height">New image height</param>
        /// <param name="mode">The resizing method.</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="upscale">All image requests allow upscaling by default. A limiter is set in the processing.config file to help prevent DoS attacks.
        /// To turn off upscaling simply append the value upscale=false to your request url. (This does not affect stretched, boxpad, nor min modes). </param>
        /// <returns></returns>
        public static UrlBuilder Resize(this UrlBuilder target, int? width, int? height, ResizeMode mode = ResizeMode.Pad, AnchorPosition anchor = AnchorPosition.Center, bool upscale = true)
        {
            return Resize(target, width, height, null, null, null, null, mode, anchor, upscale);
        }

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// ImageProcessor.Web allows you to scale images both up and down with an excellent quality to size ratio.A maximum width and height can be set in the configuration to help protect you from DoS attacks.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width">New image width</param>
        /// <param name="height">New image height</param>
        /// <param name="widthratio">New width as a ratio</param>
        /// <param name="heightratio">New height as a ratio</param>
        /// <param name="centerX">A fraction from the top-left corner of the image (x-coordinate, between 0 and 1). This point will be as close to the center of your crop as possible while keeping the crop within the image.</param>
        /// <param name="centerY">A fraction from the top-left corner of the image (y-coordinate, between 0 and 1). This point will be as close to the center of your crop as possible while keeping the crop within the image<.</param>
        /// <param name="mode">The resizing method.</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="upscale">All image requests allow upscaling by default. A limiter is set in the processing.config file to help prevent DoS attacks.
        /// To turn off upscaling simply append the value upscale=false to your request url. (This does not affect stretched, boxpad, nor min modes). </param>
        /// <returns></returns>
        public static UrlBuilder Resize(this UrlBuilder target, int? width, int? height, float? widthratio, float? heightratio, float? centerX, float? centerY, ResizeMode mode = ResizeMode.Pad, AnchorPosition anchor = AnchorPosition.Center, bool upscale = true)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                if (width != null)
                    target.QueryCollection.Add("width", width.ToString());
                if (height != null)
                    target.QueryCollection.Add("height", height.ToString());
                if (widthratio != null)
                    target.QueryCollection.Add("widthratio", widthratio.ToString());
                if (heightratio != null)
                    target.QueryCollection.Add("heightratio", heightratio.ToString());

                if (mode != ResizeMode.Pad)
                    target.QueryCollection.Add("mode", mode.ToString().ToLower());
                if (anchor != AnchorPosition.Center)
                    target.QueryCollection.Add("anchor", anchor.ToString().ToLower());
                if (upscale != true)
                    target.QueryCollection.Add("upscale", upscale.ToString().ToLower());

                if (centerX != null || centerY != null)
                {
                    centerX = centerX ?? 0;
                    centerY = centerY ?? 0;
                    target.QueryCollection.Add("center", string.Join(",", centerX.ToString(), centerY.ToString()));
                }
            }
            return target;
        }


        /// <summary>
        /// Resizes the current image to the given width, Uses defaults for all other resize parameters
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width">New width of the image</param>
        /// <returns></returns>
        public static UrlBuilder Width(this UrlBuilder target, int width)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("width", width.ToString());

            return target;
        }

        /// <summary>
        /// Resizes the current image to the given height, Uses defaults for all other resize parameters
        /// </summary>
        /// <param name="target"></param>
        /// <param name="height">New height of the image</param>
        /// <returns></returns>
        public static UrlBuilder Height(this UrlBuilder target, int height)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("height", height.ToString());

            return target;
        }

        /// <summary>
        /// Rotates the current image by the given angle without clipping.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="angle">The rotation angle</param>
        /// <returns></returns>
        public static UrlBuilder Rotate(this UrlBuilder target, float angle)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("rotate", angle.ToString());

            return target;
        }

        /// <summary>
        /// Rotates the current image by the given angle without expanding the canvas.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="angle">The rotation angle</param>
        /// <param name="keepSize">Whether to keep the image dimensions.
        /// /// <para>
        /// If set to true, the image is zoomed to fit the bounding area.
        /// </para>
        /// <para>
        /// If set to false, the area is cropped to fit the rotated image.
        /// </para>
        /// </param>
        /// <returns></returns>
        public static UrlBuilder RotateBounded(this UrlBuilder target, float angle, bool keepSize)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("rotatebounded", angle.ToString());

            if (keepSize)
                target.QueryCollection.Add("rotatebounded.keepsize", keepSize.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Adds rounded corners to the current image.
        /// Imageprocessor.Web can round the corners of your images.You can also optionally choose which corners to round. Use the background color extension to fill in the background
        /// </summary>
        /// <param name="target"></param>
        /// <param name="radius">Corner rounding radius.</param>
        /// <param name="topLeft">Round the top left corner.</param>
        /// <param name="topRight">Round the top right corner</param>
        /// <param name="bottomLeft">Round the bottom left corner</param>
        /// <param name="bottomRight">Round the bottom right corner</param>
        /// <returns></returns>
        public static UrlBuilder RoundedCorners(this UrlBuilder target, int radius, bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("roundedcorners", radius.ToString());

            if (topLeft == false)
                target.QueryCollection.Add("tl", topLeft.ToString().ToLower());

            if (topRight == false)
                target.QueryCollection.Add("tr", topRight.ToString().ToLower());

            if (bottomLeft == false)
                target.QueryCollection.Add("bl", bottomLeft.ToString().ToLower());

            if (bottomRight == false)
                target.QueryCollection.Add("br", bottomRight.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Adjusts the saturation of images.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="saturation">Percentage value (between -100 and 100)</param>
        /// <returns></returns>
        public static UrlBuilder Saturation(this UrlBuilder target, int saturation)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (saturation < -100 || saturation > 100)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("saturation", saturation.ToString());

            return target;
        }

        /// <summary>
        /// Tints the current image with the given color.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">In hex format. Needs UrlEncoded '#' if supplied, not required.</param>
        /// <returns></returns>
        public static UrlBuilder Tint(this UrlBuilder target, string color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("tint", color.ToLower());

            return target;
        }

        /// <summary>
        /// Tints the current image with the given color.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">In KnoColor format.</param>
        /// <returns></returns>
        public static UrlBuilder Tint(this UrlBuilder target, KnownColor color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("tint", color.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Tints the current image with the given color.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">In KnownColor format.</param>
        /// <returns></returns>
        public static UrlBuilder Tint(this UrlBuilder target, Color color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("tint", $"{color.R.ToString()},{color.G.ToString()},{color.B.ToString()},{color.A.ToString()}");

            return target;
        }

        /// <summary>
        /// Adds a vignette image effect to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="vignette">Default is black</param>
        /// <returns></returns>
        public static UrlBuilder Vignette(this UrlBuilder target, bool vignette = true)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("vignette", vignette.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Adds a vignette image effect to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Color of the vignette in hex format</param>
        /// <returns></returns>
        public static UrlBuilder Vignette(this UrlBuilder target, string color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("vignette", color.ToLower());

            return target;
        }


        /// <summary>
        /// Adds a vignette image effect to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Color of the vignette in KnownColor format</param>
        /// <returns></returns>
        public static UrlBuilder Vignette(this UrlBuilder target, KnownColor color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("vignette", color.ToString().ToLower());

            return target;
        }

        /// <summary>
        /// Adds a vignette image effect to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Color of the vignette in Color format</param>
        /// <returns></returns>
        public static UrlBuilder Vignette(this UrlBuilder target, Color color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("vignette", $"{color.R.ToString()},{color.G.ToString()},{color.B.ToString()},{color.A.ToString()}");

            return target;
        }


        /// <summary>
        /// Adds a text based watermark to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="position">Textposition (x,y)</param>
        /// <param name="color">Color, defaults to black</param>
        /// <param name="font">Fontfamily, defaults to SansSerif</param>
        /// <param name="size">Fontsize, defaults to 48</param>
        /// <param name="style">Fontstyle, defaults to bold</param>
        /// <param name="opacity">Fontopacity, defaults to 100</param>
        /// <param name="dropshadow">Default is false</param>
        /// <param name="vertical">Default is false</param>
        /// <param name="rtl">Default is false</param>
        /// <returns></returns>
        public static UrlBuilder Watermark(this UrlBuilder target, string text, Point? position, string color, FontFamily font = null, int size = 48, FontStyle style = FontStyle.Bold, int opacity = 100, bool dropshadow = false, bool vertical = false, bool rtl = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("watermark", text);
            }
            if (font != null)
                target.QueryCollection.Add("fontfamily", font.Name);

            if (color != null)
                target.QueryCollection.Add("color", color.ToString().ToLower());

            if (position != null)
                target.QueryCollection.Add("textposition", string.Join(",", position.Value.X.ToString(), position.Value.Y.ToString()));

            target.QueryCollection.Add("fontsize", size.ToString());
            target.QueryCollection.Add("fontstyle", style.ToString());
            target.QueryCollection.Add("fontopacity", opacity.ToString());
            target.QueryCollection.Add("dropshadow", dropshadow.ToString().ToLower());
            target.QueryCollection.Add("vertical", vertical.ToString().ToLower());
            target.QueryCollection.Add("rtl", rtl.ToString().ToLower());


            return target;
        }

        /// <summary>
        /// Adds a text based watermark to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="position">Textposition (x,y)</param>
        /// <param name="color">Color, defaults to black</param>
        /// <param name="font">Fontfamily, defaults to SansSerif</param>
        /// <param name="size">Fontsize, defaults to 48</param>
        /// <param name="style">Fontstyle, defaults to bold</param>
        /// <param name="opacity">Fontopacity, defaults to 100</param>
        /// <param name="dropshadow">Default is false</param>
        /// <param name="vertical">Default is false</param>
        /// <param name="rtl">Default is false</param>
        /// <returns></returns>
        public static UrlBuilder Watermark(this UrlBuilder target, string text, Point? position, KnownColor? color, FontFamily font = null, int size = 48, FontStyle style = FontStyle.Bold, int opacity = 100, bool dropshadow = false, bool vertical = false, bool rtl = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("watermark", text);
            }
            if (font != null)
                target.QueryCollection.Add("fontfamily", font.Name);

            if (color != null)
                target.QueryCollection.Add("color", color.ToString().ToLower());

            if (position != null)
                target.QueryCollection.Add("textposition", string.Join(",", position.Value.X.ToString(), position.Value.Y.ToString()));

            target.QueryCollection.Add("fontsize", size.ToString());
            target.QueryCollection.Add("fontstyle", style.ToString());
            target.QueryCollection.Add("fontopacity", opacity.ToString());
            target.QueryCollection.Add("dropshadow", dropshadow.ToString().ToLower());
            target.QueryCollection.Add("vertical", vertical.ToString().ToLower());
            target.QueryCollection.Add("rtl", rtl.ToString().ToLower());


            return target;
        }

        /// <summary>
        /// Adds a text based watermark to the current image.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="position">Textposition (x,y)</param>
        /// <param name="color">Color, defaults to black</param>
        /// <param name="font">Fontfamily, defaults to SansSerif</param>
        /// <param name="size">Fontsize, defaults to 48</param>
        /// <param name="style">Fontstyle, defaults to bold</param>
        /// <param name="opacity">Fontopacity, defaults to 100</param>
        /// <param name="dropshadow">Default is false</param>
        /// <param name="vertical">Default is false</param>
        /// <param name="rtl">Default is false</param>
        /// <returns></returns>
        public static UrlBuilder Watermark(this UrlBuilder target, string text, Point? position, Color? color, FontFamily font = null, int size = 48, FontStyle style = FontStyle.Bold, int opacity = 100, bool dropshadow = false, bool vertical = false, bool rtl = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("watermark", text);
            }
            if (font != null)
                target.QueryCollection.Add("fontfamily", font.Name);

            if (color != null)
                target.QueryCollection.Add("color", color.Value.ToKnownColor().ToString().ToLower());

            if (position != null)
                target.QueryCollection.Add("textposition", string.Join(",", position.Value.X.ToString(), position.Value.Y.ToString()));

            target.QueryCollection.Add("fontsize", size.ToString());
            target.QueryCollection.Add("fontstyle", style.ToString());
            target.QueryCollection.Add("fontopacity", opacity.ToString());
            target.QueryCollection.Add("dropshadow", dropshadow.ToString().ToLower());
            target.QueryCollection.Add("vertical", vertical.ToString().ToLower());
            target.QueryCollection.Add("rtl", rtl.ToString().ToLower());


            return target;
        }
    }
}
