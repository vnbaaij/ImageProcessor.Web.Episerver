The ImageProcessor.Web.Episerver family consists of four parts. Each part  is a separate nuget package, so you can pick and choose the parts you need in your site. However, as in all families there are dependencies, so adding a package will install the dependencies as well.

# ImageProcessor.Web.Episerver
This package will install the following ImageProcessor extensions:
  - IImageService implementation to read images from Episerver
  - IImageCache implementation that caches images in the configured Episerver Blob storage (the cache is self healing and cleaning) 
  - ImageProcessingModule to render processed images in edit mode.

And on top of that you also get a, strongly typed, fluent API to use in your views.  See https://world.episerver.com/blogs/vincent-baaij/dates/2017/10/episerver-and-imageprocessor-more-choice-for-developers-and-designers/ for more information

# ImageProcessor.Web.Episerver.Azure
Use the in Episerver configured Azure Storage account for storing the cached images. See https://world.episerver.com/blogs/vincent-baaij/dates/2017/11/episerver-and-imageprocessor-now-also-on-azure-and-cms-11/ for more information

# ImageProcessor.Web.Episerver.UI.Blocks
Bring the power of ImageProcessor to the Episerver Editors. See https://world.episerver.com/blogs/vincent-baaij/dates/2018/7/fggnrh/ for more information

# ImageProcessor.Web.Episerver.UI.Crop
Integrate Image Cropper Property Editors with the ImageProcessor.Web.Episerver.UI.Blocks. See https://world.episerver.com/blogs/vincent-baaij/dates/2019/1/episerver-and-imageprocessor-new-crop-addition/ for more information

## Installation 
- Install these package from the Episerver NuGet feed (http://nuget.episerver.com/feed/packages.svc/). All dependencies and necessary configuration changes will be made for you.
- You can also find the ImageProcessor.Web.Episerver.UI package in the feed. This is an older version and should not be used anymore. Use ImageProcessor.Web.Episerver.UI.Blocks instead.


## Render Image in Markup
Most convenient way to render image in markup would be use `HtmlHelper` extension method:

```
@using ImageProcessor.Web.Episerver

<img src="@Html.ProcessImage(Model.CurrentPage.MainImage).Resize(100,100)" />
```

This will make sure that markup for visitors would be (assuming that image is `png`):

```
<img src="/.../image.png?width=100&height=100">
```

And in edit mode it would generate something like this:

```
<img src="/.../image.png,,{CONTENT-ID}?epieditmode=False&width=100&height=100">
```

`ProcessImage` returns back `UrlBuilder` type, so you can fluently chain any additional paramters if needed:

```
<img src="@Html.ProcessImage(Model.CurrentPage.MainImage).Resize(100, 150).BackgroundColor("red)" />

```
Alternatively you could supply the image with all the parameters in the HTML:
```
<img src="@Url.ContentUrl(Model.Image)?width=75" />
```
See http://imageprocessor.org/imageprocessor-web/imageprocessingmodule/ for all options

## Picture Helper
This package also include an Html helper that renders a [Picture element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/picture) that let's you have responsive, optimized, and lazy loaded images.

Example usage
```
@Html.Picture(Model.Image, ImageTypes.Teaser)

@* Url (string) as input + render for progressive lazy loading *@
@Html.Picture(Url.ContentUrl(Model.Image), ImageTypes.Teaser, LazyLoadType.CustomProgressive)

@* Picture helper can be used together with the ProcessImage helper *@
@Html.Picture(Html.ProcessImage(Model.Image).ReplaceColor("fff", "f00", 99).Watermark("Episerver", new Point(100, 100), "fff"), ImageTypes.Teaser)
```
#### Parameters
* **imageReference (ContentReference) <br/>OR<br/> imageUrl (string or UrlBuilder)** <br/> 
* **imageType (ImageProcessor.Web.Episerver.ImageType)** <br/> An "Image type" defines the possible sizes and quality for an image. <br/> [Example of how to define image types](https://github.com/vnbaaij/ImageProcessor.Web.Episerver/blob/master/samples/AlloySampleLocal/Business/Rendering/ImageTypes.cs)
* **cssClass (string)** <br/> Will be added to the rendered img element.
* **layzLoadType (ImageProcessor.Web.Episerver.LazyLoadType)** <br/> 
Set lazy load type to "Native" to have a browser-native lazy loaded image. The attribute "loading='lazy'" is added to the img element.<br/>
When lazy load type is "Custom", the srcset attribute of the source element (inside the rendered picture element) will be empty, 
and an additional attribute (data-srcset) will be added that contains the image url(s). 
That enables you to lazy load the image after the rest of your page content is loaded. <br/>
When lazy load type is "CustomProgressive", the srcset attribute will contain image url(s) for a low quality version of the image, and makes it possible to lazy load the high quality image.<br/>
Lazy load type "Hybrid" render the same as "Custom", and also adds the "loading='lazy'" attribute. That means you can have native lazy loading combined with custom lazy loading for browsers that doesn't support native lazy loading.  <br/>
[Javascript example of how to lazy load the images](https://github.com/vnbaaij/ImageProcessor.Web.Episerver/blob/master/samples/AlloySampleLocal/Static/js/lazyImages.js)<br/>
* **altText (string)** <br/> Will be added to the rendered img element.<br/>
See also how to [get alt text from the image](PICTURE_HELPER_DOC.md).

The picture helper is described in more detail [here](https://hacksbyme.net/2018/10/19/a-dead-easy-way-to-optimize-the-images-on-your-episerver-site/).

### Custom rendering of a picture element
If you can't use the Picture html helper, for instance when rendering the markup client side in a React app, you can still use PictureUtils to get the data needed to render a picture element.
````
PictureUtils.GetPictureData(myImageRef, ImageTypes.Teaser)
````
GetPictureData returns a PictureData object that contains all the data needed for rendering a picture element.<br/>
GetPictureData parameters are similar to the parameters for the Picture html helper.<br/><br/>

## Change log
To get a more exact overview of the changes, you can also take a look at the commit history.
#### V5.6.2
- Actually set dependencies to use the newer ImageProcessor packages
- Give Blocks and Crop packages the same version number. No new functionallity added. 
#### V5.6.1
- Update to latest ImageProcessor (2.9) and ImageProcessor.Web (4.12) packages
#### V5.6
- Added options for rendering attribute for browser-native lazy loading.
- Adding and renaming values in Lazyloading enum. Keeping the old ones, but marked as obsolete, so no breaking change.
- Added overloading methods for Picture helper to simplify usage.
- Fixed GetCropUrl to use Width/Height instead of offsets
- Update to allow new major Episerver.Azure (v10) release
#### V5.5
- All modules updated to use the latest ImageProcessor releases

#### V5.4
- Added functionality to allow for a relative app data path as well

#### V5.3.0
- References new ImageProcessor packages
- Updated Microsoft.Data.OData (security alert) in Azure package (5.3.1) and sample site
- Fixed a bug in Azure package (5.3.2) when running multi-site environment and site-specific assets

#### V5.2.0
- New package: ImageProcessor.Web.Episerver.UI.Crop. Based on [https://github.com/itMeric/ITMeric.ImageCrop/](https://github.com/itMeric/ITMeric.ImageCrop/). 
- Added back `CropRatio` (version 5.3)
- Renamed `ImageProcessor.Web.Episerver.UI` to `ImageProcessor.Web.Episerver.UI.Blocks`.
- Added `CropProcessImageBlock` to `ImageProcessor.Web.Episerver.UI.Blocks`. Blocks package now has a dependency on Crop package.
- See [the blogpost on World](https://world.episerver.com/blogs/vincent-baaij/dates/2019/1/episerver-and-imageprocessor-new-crop-addition/) for more information

#### V5.1.0
- Basic focal point support in Picture helper
- Possible to set alt text for the generated img element inside the picture element.
- Possible to get Picture data with content reference instead of string/urlbuilder (same applies to Picture helper)
- Change of "ImageProcessorDebug" appsetting to "IPE_ShowInfo"
- Update several NuGet packages to resolve security alerts

#### V5.0.2
- Fix progressive lazyload webp bug

#### V5.0.1
- Fix regression of bug with UNC Path

#### V5.0.0
- New add-on: ImageProcessor.Web.Episerver.UI. See [the blogpost on World](https://world.episerver.com/blogs/vincent-baaij/dates/2018/7/fggnrh/)
- Minor Picture helper improvements

#### V4.2.0
- Minor Picture helper improvements
- Fixed (hopefully!) working with CDN/DXC-S

#### V4.1.0
- UNC path support is back! 

#### V4.0.0
- New major version number because of breaking change in lazy loading functionality for the Picture element

#### V3.0.1
- Don't stream blobs directly anymore. 
- Works with private containers now (DXC Service)! Now uses Shared Access Signature for downloading from blob storage. 

#### V3.0.0
- Use ImageProcessor on static files!
- Added support for lazy loading images in Picture element
- Simplifying caches (both File and Azure) and configuration
- Removed Azure specific `IImageService`
- **Changes no longer backported to CMS 10**
- See https://world.episerver.com/blogs/vincent-baaij/dates/2018/7/imageprocessor-web-episerver-new-versions/

#### V2.1.0
- Added Picture element for responsive images. See https://world.episerver.com/blogs/vincent-baaij/dates/2018/5/episever-and-imageprocessor-new-versions/

#### V2.0.0
- Added Azure Blob Storage support.
- CMS 11 support
- See https://world.episerver.com/blogs/vincent-baaij/dates/2017/11/episerver-and-imageprocessor-now-also-on-azure-and-cms-11/

#### V1.0.0
- Initial version. See https://world.episerver.com/blogs/vincent-baaij/dates/2017/10/episerver-and-imageprocessor-more-choice-for-developers-and-designers/
