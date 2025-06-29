mergeInto(LibraryManager.library, {
  SaveToLocalStorage: function(keyPtr, valuePtr) {
    var key = UTF8ToString(keyPtr);
    var value = UTF8ToString(valuePtr);
    localStorage.setItem(key, value);
  },

  LoadFromLocalStorage: function(keyPtr) {
    var key = UTF8ToString(keyPtr);
    var value = localStorage.getItem(key);
    if (!value) return 0;

    var buffer = _malloc(lengthBytesUTF8(value) + 1);
    stringToUTF8(value, buffer, lengthBytesUTF8(value) + 1);
    return buffer;
  }
});
