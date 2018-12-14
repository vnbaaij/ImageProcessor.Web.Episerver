# ImageProcessor.Web.Episerver

This package will install the following ImageProcessor extensions:
  - IImageService implementation to read images from Episerver
  - IImageCache implementation that caches images in the configured Episerver Blob storage (the cache is self healing and cleaning) 
  - ImageProcessingModule to render processed images in edit mode.

And on top of that you also get a, strongly typed, fluent API to use in your views

## Installation
- Install this package from the Episerver NuGet feed (http://nuget.episerver.com/feed/packages.svc/). All dependencies and necessary configuration changes will be made for you.


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
@Html.Picture(Url.ContentUrl(Model.Image), ImageTypes.Teaser, lazyLoadType: LazyLoadType.Progressive)

@* Picture helper can be used together with the ProcessImage helper *@
@Html.Picture(Html.ProcessImage(Model.Image).ReplaceColor("fff", "f00", 99).Watermark("Episerver", new Point(100, 100), "fff"), ImageTypes.Teaser)
```
#### Parameters
* **imageReference (ContentReference) <br/>OR<br/> imageUrl (string or UrlBuilder)** <br/> 
* **imageType (ImageProcessor.Web.Episerver.ImageType)** <br/> An "Image type" defines the possible sizes and quality for an image. <br/> [Example of how to define image types](https://github.com/vnbaaij/ImageProcessor.Web.Episerver/blob/master/samples/AlloySampleLocal/Business/Rendering/ImageTypes.cs)
* **cssClass (string)** <br/> Will be added to the rendered img element.
* **layzLoadType (ImageProcessor.Web.Episerver.LazyLoadType)** <br/> 
When lazy load type is "Regular", the srcset attribute of the source element (inside the rendered picture element) will be empty, 
and an additional attribute (data-srcset) will be added that contains the image url(s). 
That enables you to lazy load the image after the rest of your page content is loaded. <br/>
When lazy load type is "Progressive", the srcset attribute will contain image url(s) for a low quality version of the image, and makes it possible to lazy load the high quality image.<br/>
[Javascript example of how to lazy load the images](https://github.com/vnbaaij/ImageProcessor.Web.Episerver/blob/master/samples/AlloySampleLocal/Static/js/lazyImages.js)<br/>
* **altText (string)** <br/> Will be added to the rendered img element.<br/>
See also how to [get alt text from the image](picture_helper_doc.md).

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
