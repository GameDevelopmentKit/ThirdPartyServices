mergeInto(LibraryManager.library, {
    FetchRemoteConfig: function (callbackObj, callbackFunc) {
        const callback = UTF8ToString(callbackObj);
        const callFunc = UTF8ToString(callbackFunc);
        firebaseRemoteConfigFetch(firebaseRemoteConfig)
            .then(() => {
                // ...                              
                console.log("firebase init complete " + (callback) + " " + (callFunc))
                SendMessage(callback, callFunc, "")
            })
            .catch((err) => {
                // ...
            });

    },

    GetRemoteConfigValue: function (key) {
        const remoteKey = UTF8ToString(key);
        const val = firebaseRemoteConfigGetValue(firebaseRemoteConfig, remoteKey);
        var size = lengthBytesUTF8(val._value) + 1;
        var buffer = _malloc(size);
        stringToUTF8(val._value, buffer, size);
        return buffer;
    },

});

