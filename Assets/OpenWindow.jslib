var OpenWindowPlugin = {
    openWindow: function(link)
    {
        var url = Pointer_stringify(link);
        document.onmouseup = function()
        {
            window.location.assign(url);
            document.onmouseup = null;
        }
    }
};

mergeInto(LibraryManager.library, OpenWindowPlugin);