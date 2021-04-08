export function initMagnificPopup() {
    $('.attachments-gallery').magnificPopup({
        delegate: 'a',
        type: 'image',
        closeOnContentClick: true,
        closeBtnInside: false,
        mainClass: 'mfp-with-zoom mfp-img-mobile',
        image: {
            verticalFit: true
        },
        gallery: {
            enabled: true
        },
        zoom: {
            enabled: true,
            duration: 200,
            opener: function(element) {
                return element.find('img');
            }
        }
    });
}