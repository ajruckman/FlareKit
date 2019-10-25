console.log('root');

window.provision = function (dotnetHelper, targetID) {
    console.log("provision " + targetID);

    // https://stackoverflow.com/a/7385673/9911189
    let container = $('#' + targetID);
    console.log(container);
    
    $(document).mouseup(function (e) {
        // if the target of the click isn't the container nor a descendant of the container
        if (!container.is(e.target) && container.has(e.target).length === 0)
        {
            console.log('click outer ' + targetID);
            dotnetHelper.invokeMethodAsync('OuterClick', targetID);
        } else {
            console.log('click inner ' + targetID);
            dotnetHelper.invokeMethodAsync('InnerClick', targetID);
        }
    });
};

// window.provision = function (dotnetHelper, id) {
//     console.log("provision " + id);
//
//     window.hideOnClickOutside(dotnetHelper, document.getElementById(id));
// };

// window.hideOnClickOutside = function (dotnetHelper, element) {
//     console.log(element);
//
//     const outsideClickListener = event => {
//         if (!element.contains(event.target) && window.isVisible(element)) { // or use: event.target.closest(selector) === null
//             dotnetHelper.invokeMethodAsync("OuterClick");
//             //element.style.display = 'none'
//             //removeClickListener()
//         }
//     };
//
//     const removeClickListener = () => {
//         document.removeEventListener("click", outsideClickListener);
//     };
//
//     document.addEventListener("click", outsideClickListener);
// };
//
// // source (2018-03-11): https://github.com/jquery/jquery/blob/master/src/css/hiddenVisibleSelectors.js";
// window.isVisible = elem => !!elem && !!(elem.offsetWidth || elem.offsetHeight || elem.getClientRects().length); 