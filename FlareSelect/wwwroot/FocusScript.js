window.FlareSelect = {
    focusElement: function (id) {
        setTimeout(function () {
            document.getElementById(id).focus();
        }, 10);
    },
};
