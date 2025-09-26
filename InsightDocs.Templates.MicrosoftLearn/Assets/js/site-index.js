async function onSiteIndexLoad() {
    let useDynamicToc = document.body.getAttribute('data-use-dynamic-toc') == 'true';
    let tocUrl = document.body.getAttribute('data-toc-url');
    let tocData = null;

    let navbar = document.getElementById('navbar');
    let topicContent = document.getElementById('topicContentIFrame');
    let tocItemLookup = {};

    function addTocItemChildren(indices, container) {
        if (!indices) {
            return;
        }

        for (let index of indices) {
            let tocItem = tocData.Items[index];
            let tocElement = document.createElement('li');
            let tocElementLabel = null;

            tocElement.classList.add('tree-item');

            if (tocItem.c && tocItem.c.length > 0) {
                tocElementLabel = document.createElement('span');
                tocElementLabel.classList.add('tree-expander');

                if (tocItem.u) {
                    tocElementLabel.innerHTML = `<span class="tree-expander-indicator docon docon-chevron-right-light" aria-hidden="true"></span><a class="tree-item" href="${tocItem.u}" target="topicContentIFrame">${tocItem.t.replace(/\</g, '&lt;').replace(/\>/g, '&gt;')}</a>`;
                }

                else {
                    tocElementLabel.innerHTML = `<span class="tree-expander-indicator docon docon-chevron-right-light" aria-hidden="true"></span>${tocItem.t.replace(/\</g, '&lt;').replace(/\>/g, '&gt;')}`;
                }
            }

            else {
                if (tocItem.u) {
                    tocElementLabel = document.createElement('a');
                    tocElementLabel.classList.add('tree-item');
                    tocElementLabel.target = 'topicContentIFrame';
                    tocElementLabel.href = tocItem.u;
                }

                else {
                    tocElementLabel = document.createElement('span');
                    tocElementLabel.classList.add('tree-expander');
                }

                tocElementLabel.innerHTML = tocItem.t.replace(/\</g, '&lt;').replace(/\>/g, '&gt;');
            }

            tocElement.appendChild(tocElementLabel);
            tocElement.setAttribute('data-toc-item-index', index);

            if (!tocItem.c || tocItem.c.length === 0) {
                tocElementLabel.classList.add('is-leaf');
            }

            tocElement.appendChild(tocElementLabel);
            container.appendChild(tocElement);

            tocItemLookup[index.toString()] = tocElement;
        }
    }

    navbar.addEventListener('click', async (evt) => {
        let target = null;

        if (evt.target.tagName === 'LI') {
            target = evt.target;
        }

        else if (evt.target.tagName === 'A' && evt.target.classList.contains('tree-item')) {
            target = evt.target.classList.contains('is-leaf') ? evt.target.parentElement : evt.target.parentElement.parentElement;
        }

        else if (evt.target.classList.contains('tree-expander') || evt.target.classList.contains('tree-item')) {
            target = evt.target.parentElement;
        }

        else if (evt.target.classList.contains('tree-expander-indicator')) {
            target = evt.target.parentElement.parentElement;
        }

        if (!target || target.classList.contains('is-loading')) {
            return;
        }

        if (!target.classList.contains('is-leaf') && !target.classList.contains('is-expanded') && !target.classList.contains('is-collapsed')) {
            let childList = document.createElement('ul');
            childList.classList.add('tree-group');

            target.appendChild(childList);

            let itemIndex = parseInt(target.getAttribute('data-toc-item-index'));
            let childIndices = tocData.Items[itemIndex].c;

            if (useDynamicToc && childIndices && childIndices.length > 0 && !tocData.Items[childIndices[0]]) {
                target.classList.add('is-loading');

                let childrenResponse = await fetch(`${tocUrl}/children/${itemIndex}`);
                let childrenData = await childrenResponse.json();

                for (let childIndexString of Object.keys(childrenData)) {
                    let childIndex = parseInt(childIndexString);
                    tocData.Items[childIndex] = childrenData[childIndex];

                    if (childrenData[childIndex].u) {
                        tocData.UrlLookups[childrenData[childIndex].u] = childIndex;
                    }
                }

                target.classList.remove('is-loading');
            }

            addTocItemChildren(childIndices, childList);
            target.classList.add('is-collapsed');
        }

        if (target.classList.contains('is-collapsed')) {
            target.classList.add('is-expanded');
            target.classList.remove('is-collapsed');
        }

        else if (target.classList.contains('is-expanded')) {
            target.classList.remove('is-expanded');
            target.classList.add('is-collapsed');
        }
    });

    function selectNavbarListItem(url) {
        let tocItemIndex = tocData.UrlLookups[url];

        if (typeof tocItemIndex === 'undefined') {
            return;
        }

        if (!tocItemLookup[tocItemIndex.toString()] || (tocItemLookup[tocItemIndex.toString()].lastElementChild.tagName !== 'UL' && tocData.Items[tocItemIndex].c)) {
            let tocItem = tocData.Items[tocItemIndex];

            if (!tocItem) {
                return;
            }

            let tocItemHierarchy = [tocItemIndex];
            let currentParentIndex = tocItem.p;

            while (typeof currentParentIndex !== 'undefined') {
                tocItemHierarchy.splice(0, 0, currentParentIndex);
                currentParentIndex = tocData.Items[currentParentIndex].p;
            }

            let i = 0;

            for (; i < tocItemHierarchy.length; i++) {
                let currentTocItem = tocItemLookup[tocItemHierarchy[i].toString()];

                if (!currentTocItem) {
                    break;
                }
            }

            if (i < tocItemHierarchy.length) {
                for (let x = i - 1; x < tocItemHierarchy.length; x++) {
                    let currentTocItem = tocData.Items[tocItemHierarchy[x]];
                    let currentContainer = tocItemLookup[tocItemHierarchy[x]];

                    let childList = document.createElement('ul');
                    childList.classList.add('tree-group');

                    currentContainer.appendChild(childList);

                    addTocItemChildren(currentTocItem.c, childList);
                    currentContainer.classList.add('is-expanded');
                    currentContainer.classList.remove('is-collapsed');
                }
            }

            else if (i === 1 && tocItemHierarchy.length === 1 && tocItemLookup[tocItemIndex.toString()].lastElementChild.tagName !== 'UL' && tocData.Items[tocItemIndex].c) {
                let childList = document.createElement('ul');
                childList.classList.add('tree-group');

                tocItemLookup[tocItemIndex.toString()].appendChild(childList);
                addTocItemChildren(tocData.Items[tocItemIndex].c, childList);
            }
        }

        let currentTocItemData = tocData.Items[tocItemIndex];
        let currentTocItem = tocItemLookup[tocItemIndex.toString()];

        while (currentTocItem) {
            if (!currentTocItem.classList.contains('is-leaf')) {
                currentTocItem.classList.add('is-expanded');
                currentTocItem.classList.remove('is-collapsed');
            }

            currentTocItem = typeof currentTocItemData.p === 'undefined' ? null : tocItemLookup[currentTocItemData.p.toString()];
            currentTocItemData = typeof currentTocItemData.p === 'undefined' ? null : tocData.Items[currentTocItemData.p];
        }

        navbar.querySelectorAll('.is-selected').forEach(e => e.classList.remove('is-selected'));

        if (tocItemLookup[tocItemIndex.toString()].classList.contains('tree-item')) {
            tocItemLookup[tocItemIndex.toString()].classList.add('is-selected');
        }

        else {
            tocItemLookup[tocItemIndex.toString()].querySelector('.tree-item').classList.add('is-selected');
        }

        let tocItem = tocItemLookup[tocItemIndex.toString()];
        let navbarClientHeight = navbar.clientHeight;

        if (tocItem.offsetTop < navbar.scrollTop || tocItem.offsetTop > navbar.scrollTop + navbarClientHeight) {
            navbar.scrollTop = tocItem.offsetTop - (navbarClientHeight / 2);
        }

        navbar.dispatchEvent(new CustomEvent('tocitemselected', {
            detail: {
                tocItem: tocItem,
                data: tocData.Items[tocItemIndex],
                allData: tocData
            }
        }));
    }

    topicContent.addEventListener('load', async () => {
        if (topicContent.contentDocument.location.href !== 'about:blank' && document.location.hash !== '#' + topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash.replace('#', '%23')) {
            document.location.replace('#' + topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash.replace('#', '%23'));
        }

        if (useDynamicToc && !tocData.UrlLookups[topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash] && !tocData.UrlLookups[topicContent.contentDocument.location.pathname]) {
            let indexResponse = await fetch(`${tocUrl}/index?url=${encodeURIComponent(topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash)}`);
            let index = await indexResponse.json();

            if (index >= 0) {
                tocData.UrlLookups[topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash] = index;

                let ancestorsResponse = await fetch(`${tocUrl}/ancestors/${index}`);
                let ancestors = await ancestorsResponse.json();
                let ancestorsToFetch = [];

                for (let ancestorIndex of ancestors) {
                    if (!tocData.Items[ancestorIndex] || (tocData.Items[ancestorIndex].c && tocData.Items[ancestorIndex].c.length > 0 && !tocData.Items[tocData.Items[ancestorIndex].c[0]])) {
                        ancestorsToFetch.push(ancestorIndex);
                    }
                }

                let ancestorChildrenResponse = await fetch(`${tocUrl}/children`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(ancestorsToFetch)
                });

                let ancestorChildren = await ancestorChildrenResponse.json();

                for (let indexString of Object.keys(ancestorChildren)) {
                    let index = parseInt(indexString);
                    tocData.Items[index] = ancestorChildren[index];

                    if (ancestorChildren[index].u) {
                        tocData.UrlLookups[ancestorChildren[index].u] = index;
                    }
                }
            }
        }

        selectNavbarListItem(tocData.UrlLookups[topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash] ? topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash : topicContent.contentDocument.location.pathname);

        topicContent.contentWindow.addEventListener('hashchange', () => {
            if (topicContent.contentDocument.location.href !== 'about:blank' && document.location.hash !== '#' + topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash.replace('#', '%23')) {
                document.location.replace('#' + topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash.replace('#', '%23'));
                selectNavbarListItem(tocData.UrlLookups[topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash] ? topicContent.contentDocument.location.pathname + topicContent.contentDocument.location.hash : topicContent.contentDocument.location.pathname);
            }
        });
    });

    let navbarRootList = document.createElement('ul');
    navbarRootList.classList.add('tree', 'table-of-contents', 'flex-grow-1', 'flex-shrink-1');

    navbar.appendChild(navbarRootList);

    if (useDynamicToc) {
        tocData = {
            Items: [],
            UrlLookups: {},
            RootItems: []
        };

        let rootItemsResponse = await fetch(`${tocUrl}/root-items`);
        let rootItems = await rootItemsResponse.json();

        for (let rootItemIndexString of Object.keys(rootItems)) {
            let rootItemIndex = parseInt(rootItemIndexString);

            tocData.RootItems.push(rootItemIndex);
            tocData.Items[rootItemIndex] = rootItems[rootItemIndex];

            if (rootItems[rootItemIndex].u) {
                tocData.UrlLookups[rootItems[rootItemIndex].u] = rootItemIndex;
            }
        }
    }

    else {
        let tocResponse = await fetch(tocUrl);
        tocData = await tocResponse.json();
    }

    addTocItemChildren(tocData.RootItems, navbarRootList);

    if (document.location.hash) {
        topicContent.src = document.location.hash.substring(1).replace('%23', '#');
    }
}

function printTopic() {
    document.getElementById('topicContentIFrame').contentWindow.print();
}

/**
 * @param {MouseEvent} evt2
 */
function navbarStartResize(evt2) {
    if (evt2.button === 0) {
        let navbar = document.getElementById('navbar');
        let topicContentIFrame = document.getElementById('topicContentIFrame');
        let navbarStyle = window.getComputedStyle(navbar);
        let minWidth = parseFloat(navbarStyle.minWidth || '0');
        let maxWidth = parseFloat(navbarStyle.maxWidth || document.body.clientWidth.toString());
        let currentWidth = parseFloat(navbarStyle.width);

        let navbarResizerDrag =
            /**
             * @param {MouseEvent} evt
             */
            function (evt) {
                currentWidth += evt.movementX;

                if (currentWidth > maxWidth) {
                    currentWidth = maxWidth;
                }

                if (currentWidth < minWidth) {
                    currentWidth = minWidth;
                }

                navbar.style.width = currentWidth + 'px';
            };

        let navbarResizerMouseUp =
            /**
             * @param {MouseEvent} evt
             */
            function (evt) {
                if (evt.button === 0) {
                    document.removeEventListener('mouseup', navbarResizerMouseUp);
                    document.removeEventListener('mousemove', navbarResizerDrag);
                    topicContentIFrame.contentWindow.document.removeEventListener('mouseup', navbarResizerMouseUp);
                    topicContentIFrame.contentWindow.document.removeEventListener('mousemove', navbarResizerDrag);
                }
            };

        document.addEventListener('mousemove', navbarResizerDrag);
        document.addEventListener('mouseup', navbarResizerMouseUp);
        topicContentIFrame.contentWindow.document.addEventListener('mousemove', navbarResizerDrag);
        topicContentIFrame.contentWindow.document.addEventListener('mouseup', navbarResizerMouseUp);
    }
}