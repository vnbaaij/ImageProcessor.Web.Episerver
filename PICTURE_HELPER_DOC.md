# Picture Helper examples

## alt text
There are two ways to specify what should be in the img-element's alt attribute. 
#### Use altText parameter<br/>
````
@Html.Picture(Model.Image, ImageTypes.Teaser, altText: "My alt text")

@* Also possible to use the url (string) as parameter, instead of an image reference *@
@Html.Picture(Url.ContentUrl(Model.Image), ImageTypes.Teaser, altText: "My alt text")
````

#### Get alt text from the image
It's also possible to get the alt text from the actual image (IContent).<br/>
Add a property named "ImageAltText" to your image file model
````
[Display(Name = "Alt text")]
public virtual string ImageAltText { get; set; }
````
and add the following appsetting in web.config
````
<add key="IPE_AltTextFromImage" value="true" />
````
The alt text will be rendered when using the Picture helper
````
@Html.Picture(Model.Image, ImageTypes.Teaser)
```` 
Note that it is necessary to use the image reference as parameter when fetching the alt text from the image.