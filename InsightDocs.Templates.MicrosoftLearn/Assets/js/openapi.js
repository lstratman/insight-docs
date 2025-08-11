function addSchemaReferenceLinks() {
    if (!window['schemaReferenceUrls']) {
        return;
    }

    document.querySelectorAll('.language-json span.hljs-attr').forEach(e => {
        if (e.innerHTML != '"$ref"') {
            return;
        }

        let referenceElement = e.nextElementSibling.nextElementSibling;
        let reference = referenceElement.innerHTML.replace(/\"/g, '');
        let url = window['schemaReferenceUrls'][reference];

        if (url) {
            let openQuote = document.createElement('span');
            openQuote.className = 'hljs-string';
            openQuote.innerHTML = '"';

            let referenceLink = document.createElement('a');
            referenceLink.href = url;
            referenceLink.innerHTML = reference;

            let closeQuote = document.createElement('span');
            closeQuote.className = 'hljs-string';
            closeQuote.innerHTML = '"';

            referenceElement.insertAdjacentElement('beforebegin', openQuote);
            referenceElement.insertAdjacentElement('beforebegin', referenceLink);
            referenceElement.insertAdjacentElement('beforebegin', closeQuote);

            referenceElement.parentElement.removeChild(referenceElement);
        }
    });

    document.querySelectorAll('.language-xml span.hljs-attr').forEach(e => {
        if (e.innerHTML != 'type') {
            return;
        }

        let referenceElement = e.nextElementSibling;
        let reference = referenceElement.innerHTML.replace(/\"/g, '');
        let url = window['schemaReferenceUrls'][reference];

        if (url) {
            let openQuote = document.createElement('span');
            openQuote.className = 'hljs-string';
            openQuote.innerHTML = '"';

            let referenceLink = document.createElement('a');
            referenceLink.href = url;
            referenceLink.innerHTML = reference;

            let closeQuote = document.createElement('span');
            closeQuote.className = 'hljs-string';
            closeQuote.innerHTML = '"';

            referenceElement.insertAdjacentElement('beforebegin', openQuote);
            referenceElement.insertAdjacentElement('beforebegin', referenceLink);
            referenceElement.insertAdjacentElement('beforebegin', closeQuote);

            referenceElement.parentElement.removeChild(referenceElement);
        }
    });
}