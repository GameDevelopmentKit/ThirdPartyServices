var FBInstantPlayerLibrary = {

    $GetBuffer: function (str) {
        str = str || "";
        var size = lengthBytesUTF8(str) + 1;
        var buffer = _malloc(size);
        stringToUTF8(str, buffer, size);
        return buffer;
    },

    GetUserId: function () {
        return GetBuffer(FBInstant.player.getID());
    },

    GetUserName: function () {
        return GetBuffer(FBInstant.player.getName());
    },

    GetUserAvatar: function () {
        return GetBuffer(FBInstant.player.getPhoto());
    },

    SaveUserData: function (json, callbackObj, callbackFunc, callbackId) {
        json = JSON.parse(UTF8ToString(json));
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);
        callbackId = UTF8ToString(callbackId);

        FBInstant.player.setDataAsync(json)
            .then(FBInstant.player.flushDataAsync)
            .then(() => {
                console.log("save user data ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: null, id: callbackId }));
            })
            .catch(err => {
                console.error("save user data error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: err.message || "", id: callbackId }));
            });
    },

    LoadUserData: function (keys, callbackObj, callbackFunc, callbackId) {
        keys = JSON.parse(UTF8ToString(keys));
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);
        callbackId = UTF8ToString(callbackId);

        FBInstant.player.getDataAsync(keys)
            .then(data => {
                console.log("load user data ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ data: JSON.stringify(keys.map(key => JSON.stringify(data[key]))), error: null, id: callbackId }));
            })
            .catch(err => {
                console.error("load user data error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ data: null, error: err.message || "", id: callbackId }));
            });
    },
};

autoAddDeps(FBInstantPlayerLibrary, "$GetBuffer");
mergeInto(LibraryManager.library, FBInstantPlayerLibrary);
