console.log('root');

function toggle() {
    
}

window.provision = function (dotnetHelper, targetID) {
    console.log("provision " + targetID);

    // https://stackoverflow.com/a/7385673/9911189
    let container = $('#' + targetID);
    console.log(container);
    
    // container.find('.FlareSelect_OptionContainer').keyup(function(e) {
    //     if (e.which !== 40) {
    //         // return;
    //     }
    //    
    //
    // });
    
    container.keydown(function(e) { 
        if (e.which === 38 || e.which === 40) {
            console.log('block');
            e.preventDefault();
            e.stopPropagation();
        }
    })
    
    $(document).mouseup(function (e) {
        // if the target of the click isn't the container nor a descendant of the container
        if (!container.is(e.target) && container.has(e.target).length === 0)
        {
            console.log('click outer ' + targetID);
            dotnetHelper.invokeMethodAsync('OuterClick', targetID);
        } else {
            console.log('click inner ' + targetID);
            dotnetHelper.invokeMethodAsync('InnerClick', targetID).then(() => {
                container.find('.FlareSelect_Search').get(0).focus();
            });
        }
    });
    
    $(document).keyup(function(e) {
        if (e.which === 13) {
            // if the target of the click isn't the container nor a descendant of the container
            if (!container.is(e.target) && container.has(e.target).length === 0)
            {
                console.log('click outer ' + targetID);
                dotnetHelper.invokeMethodAsync('OuterClick', targetID);
            } else {
                console.log('click inner ' + targetID);
                dotnetHelper.invokeMethodAsync('InnerClick', targetID);
            }
        }
    });
    
    container.keyup(function(e) {
        console.log(e.which);
        
        if (e.which === 38 || e.which === 40) {
            let selected = container.find('.FlareSelect_OptionContainer :focus');
                       
            console.log(selected);
            
            if (!selected.length) {
                console.log("unfound");
                
                selected = container.find('.FlareSelect_Option');
                if (!selected.length) {
                    console.log("unfound x2");
                    return;
                }
                
                selected.get(0).focus();
                return;
            }
            
            let target = selected.get(0);
            console.log(target);
            
            let tabIndex = $(target).attr('tabindex');
            console.log(tabIndex);
            
            if (e.which === 38) {
                tabIndex--;
            } else if (e.which === 40) {
                tabIndex++;
            }
            let next = container.find('.FlareSelect_Option[tabindex=' + tabIndex + ']');
            next.focus();
            
            console.log();
            // let tabIndex = $(this).attr('tabindex');
            // tabIndex++;
            // $('[tabindex=' + tabIndex + ']').focus();
            // console.log('-> ' + tabIndex)
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