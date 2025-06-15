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