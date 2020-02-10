
function initializeLazyImages() {
  // Get images that has the data-src attribute
  const lazyImages = document.querySelectorAll('img[data-src]');

  // Just load the images if browser doesn't support IntersectionObserver (Assuming that there is no browser that supports native lazy loading, but doesn't support IntersectionObserver). 
  if (!('IntersectionObserver' in window)) {
    lazyImages.forEach((imageElement) => setSrcAttribute(imageElement));
    return;
  }

  // Create observer
  const observer = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (entry.intersectionRatio <= 0) {
          return;
        }

        const imageElement = entry.target;
        setSrcAttribute(imageElement);
        observer.unobserve(imageElement);
      });
    },
    // Configuration for the observer
    { rootMargin: '100px' } // load images if it gets within 100px
  ); 


  lazyImages.forEach((imageElement) => {
    if (imageElement.loading === 'lazy' && 'loading' in HTMLImageElement.prototype) {
      // Set src attribute if image element is using native lazy loading (and the browser supports it)
      setSrcAttribute(imageElement);
    } else {
      // Let the image be observed
      observer.observe(imageElement);
    }
  });
}

function setSrcAttribute(imageElement) {
  // If image is within a picture element, set srcset attribute for all source elements
  const parent = imageElement.parentNode;
  if (parent.tagName === 'PICTURE') {
    const sourceElements = parent.querySelectorAll('source');
    sourceElements.forEach((sourceElement) => {
      sourceElement.srcset = sourceElement.dataset.srcset;
    });
  }

  // Set img src attribute
  imageElement.src = imageElement.dataset.src;
  console.log(imageElement.src);

}