
mergeInto(LibraryManager.library, {

  JsAlert: function(msg) {
    window.alert(Pointer_stringify(msg));
  },
  
  JsOpenWindow: function(link) {
    var url = Pointer_stringify(link);
    document.onmouseup = function()
    {
        window.location.assign(url);
        document.onmouseup = null;
    };
  },
  
  JsCreateHiddenFileInput: function() {
    var inputId = "file";
    var appendTo = "gameContainer";
    
    var input = document.createElement("input");
    input.setAttribute("type", "file");
    input.setAttribute("name", inputId);
    input.setAttribute("id", inputId);
    // hidden inputs can't be 'clicked' via Javascript
    // input.setAttribute("hidden", true);
    input.style.position = "absolute";
    input.style.display = "none";
    input.style.cursor = "inherit";
    input.style.top = "-1000px";
    input.style.right = "-1000px";
    input.style.minWidth = "100%";
    input.style.minHeight = "100%";
    input.style.opacity = 0;
    input.style.filter = "alpha(opacity=0)";
    document.getElementById(appendTo).appendChild(input);
    
    document.getElementById(inputId).onchange = function(e) {
        console.log('file input onchange');
        var loadedFile = e.srcElement.files[0];
        var filename = loadedFile.name;
        var reader = new FileReader();
        reader.onload = function(data) {
            console.log('file input reader onload');
            var json = JSON.stringify({"filename": filename, "data": data.target.result});
            SendMessage("ScriptHolder", "OnReceiveUpload", json);
        };
        reader.readAsText(loadedFile);
        e.target.value = null;
    };
  },
  
  JsShowFileInput: function() {
    var inputId = "file";
    var input = document.getElementById(inputId);
    input.focus();
    input.click();
    document.getElementById("#canvas").focus();
  },
});