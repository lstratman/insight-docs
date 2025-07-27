function ancestorNodeByClass(node, className) {
    if (node.classList.contains(className)) {
        return node;
    }

    if (node.parentElement) {
        return ancestorNodeByClass(node.parentElement, className);
    }

    return null;
}

function getCookie(name) {
    const cookies = document.cookie ? document.cookie.split(';') : [];
    const prefix = `${name}=`;

    for (let cookie of cookies) {
        cookie = cookie.trim();

        if (cookie.startsWith(prefix)) {
            return decodeURIComponent(cookie.substring(prefix.length));
        }
    }

    return null;
}

/**
 * @param {MouseEvent} evt
 */
async function copyCodeBlock(evt) {
    let codeBlock = evt.currentTarget.parentElement.nextElementSibling.firstElementChild;
    let successIcon = evt.currentTarget.lastElementChild;

    await copyTextFromNode(codeBlock);
    successIcon.classList.remove('is-transparent');

    setTimeout(function () {
        successIcon.classList.add('is-transparent');
    }, 1000);
}

async function copyTextFromNode(node) {
    const text = node.textContent;

    try {
        await navigator.clipboard.writeText(text);
    }

    catch (err) {
        // Fallback for older browsers
        const textarea = document.createElement("textarea");
        textarea.value = text;
        textarea.style.position = "fixed";
        textarea.style.opacity = "0";

        document.body.appendChild(textarea);

        textarea.focus();
        textarea.select();

        try {
            document.execCommand("copy");
        }

        catch (err) {
        }

        document.body.removeChild(textarea);
    }
}
