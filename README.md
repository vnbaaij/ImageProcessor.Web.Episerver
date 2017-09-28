## ImageProcessor.Web.Episerver

This package will install the following ImageProcessor extensions:
  - IImageService implementation to read images from Episerver
  - IImageCache implementation that caches images in the configured Episerver Blob storage (the cache is self healing and cleaning) 
  - ImageProcessingModule to render processed images in edit mode.

## Installation
- Install ImageProcessor.Web (will include ImageProcessor Core package)
- Install ImageProcessor.Web.Config (so we can override default ImageProcessor cache settings)
- Install this package


## Render Image in Markup**
Most convenient way to render image in markup would be use `HtmlHelper` extension method:

```
@using ImageProcessor.Web.Episerver

<img src="@Html.ResizeImage(Model.CurrentPage.MainImage, 100, 100)" />
```

This will make sure that markup for visitors would be (assuming that image is `png`):

```
<image src="/.../image.png?width=100&height=100">
```

And in edit mode it would generate something like this:

```
<image src="/.../image.png,,{CONTENT-ID}?epieditmode=False&width=100&height=100">
```

`ResizeImage` returns back `UrlBuilder` type, so you can fluently chain any additional paramters if needed:

```
<img src="@Html.ResizeImage(Model.CurrentPage.MainImage, 100, 150).Add("gradient", "true").Add("bgcolor", "red)" />

```
Alternatively you could supply the image with parameters in the HTML:
```
<img src="@Url.ContentUrl(Model.Image)?width=75" />
```
** Extensions are copied from https://github.com/valdisiljuconoks/ImageResizer.Plugins.EPiServerBlobReader 
