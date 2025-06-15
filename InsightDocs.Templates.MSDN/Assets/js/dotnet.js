function toggleLanguageMenu(event) {
    if (event.currentTarget.getAttribute('aria-expanded') === 'true') {
        event.currentTarget.setAttribute('aria-expanded', 'false');
    }

    else {
        event.currentTarget.setAttribute('aria-expanded', 'true');

        let languageMenuButton = event.currentTarget;

        let closeLanguageMenu = function (e) {
            if (!ancestorNodeByClass(e.target, 'language-selector')) {
                languageMenuButton.setAttribute('aria-expanded', 'false');
                document.body.removeEventListener('click', closeLanguageMenu);
            }
        }

        setTimeout(function () {
            document.body.addEventListener('click', closeLanguageMenu);
        }, 50);
    }
}

function setCurrentLanguage(event) {
    _setCurrentLanguage(event.currentTarget.innerText);
    document.cookie = "currentLanguage=" + event.currentTarget.innerText;
}

function _setCurrentLanguage(language) {
    let languageDropdownLabel = document.querySelector('.language.dropdown').firstElementChild.firstElementChild;
    languageDropdownLabel.innerText = language;

    document.querySelectorAll('.codeHeader').forEach(e => {
        let codeHeaderLanguage = e.firstElementChild.innerText;

        if (codeHeaderLanguage) {
            if (codeHeaderLanguage !== language) {
                e.setAttribute('hidden', null);
                e.nextElementSibling.setAttribute('hidden', null);
            }

            else {
                e.removeAttribute('hidden');
                e.nextElementSibling.removeAttribute('hidden');
            }
        }
    });
}

if (getCookie('currentLanguage')) {
    window.addEventListener('load', function () {
        _setCurrentLanguage(getCookie('currentLanguage'));
    });
}