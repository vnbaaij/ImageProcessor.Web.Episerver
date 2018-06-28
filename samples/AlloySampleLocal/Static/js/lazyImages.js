


function initializeLazyImages() {
  // Get images that has the data-src attribute
  const lazyImages = document.querySelectorAll('img[data-src]');

  // If browser doesn't support IntersectionObserver, load all images 
  if (!('IntersectionObserver' in window)) {
    lazyImages.forEach((imageElement) => loadImage(imageElement));
    return;
  }

  // Create observer
  const observer = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (entry.intersectionRatio <= 0) {
          return;
        }

        const imageElement = entry.target;
        loadImage(imageElement);
        observer.unobserve(imageElement);
      });
    },
    // Configuration for the observer
    { rootMargin: '100px' } // load images if it gets within 100px
  ); 

  // Let all lazy images be observered
  lazyImages.forEach((imageElement) => {
    observer.observe(imageElement);
  });
}

function loadImage(imageElement) {
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
}

initializeLazyImages();
