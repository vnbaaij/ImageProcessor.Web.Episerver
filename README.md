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