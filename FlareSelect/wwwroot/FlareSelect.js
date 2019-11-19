// Sources
// - https://stackoverflow.com/a/7385673/9911189

if (window.FlareSelect_Containers == null) {
    window.FlareSelect_Containers = [];

    $(document).mouseup(function (e) {
        for (let key in window.FlareSelect_Containers) {
            if (!window.FlareSelect_Containers.hasOwnProperty(key)) {
                continue;
            }

            let targetID = key;
            let container = window.FlareSelect_Containers[targetID];
            let dotnetHelper = container.dotnetHelper;

            if (!container.is(e.target) && container.has(e.target).length === 0) {
                dotnetHelper.invokeMethodAsync("OuterClick", targetID);
            } else {
                dotnetHelper.invokeMethodAsync("InnerClick", targetID).then(() => {
                    container.find(".FlareSelect_Search").get(0).focus();
                });
            }
        }
    });

    $(document).keyup(function (e) {
        if (e.which === 13) {
            for (let key in window.FlareSelect_Containers) {
                if (!window.FlareSelect_Containers.hasOwnProperty(key)) {
                    continue;
                }

                let targetID = key;
                let container = window.FlareSelect_Containers[targetID];
                let dotnetHelper = container.dotnetHelper;

                if (!container.is(e.target) && container.has(e.target).length === 0) {
                    dotnetHelper.invokeMethodAsync("OuterClick", targetID);
                } else {
                    dotnetHelper.invokeMethodAsync("InnerClick", targetID);
                }
            }
        }
    });
}

window.provision = function (dotnetHelper, targetID) {
    let container = $("#" + targetID);

    container.keydown(function (e) {
        if (e.which === 38 || e.which === 40) {
            e.preventDefault();
            e.stopPropagation();
        }
    });

    container.keyup(function (e) {
        if (e.which === 38 || e.which === 40) {
            let selected = container.find(".FlareSelect_OptionContainer :focus");

            if (!selected.length) {
                selected = container.find(".FlareSelect_Option");
                if (!selected.length) {
                    return;
                }

                selected.get(0).focus();
                return;
            }

            let target = selected.get(0);
            let tabIndex = $(target).attr("tabindex");

            if (e.which === 38) {
                tabIndex--;
            } else if (e.which === 40) {
                tabIndex++;
            }

            let next = container.find(".FlareSelect_Option[tabindex=" + tabIndex + "]");
            next.focus();
        } else if (e.which === 27) {
            // On escape
            
            container.find('.FlareSelect_Search').val('');
            dotnetHelper.invokeMethodAsync("OuterClick", targetID);
        }
    });

    window.FlareSelect_Containers[targetID] = container;
    window.FlareSelect_Containers[targetID].dotnetHelper = dotnetHelper;
};
