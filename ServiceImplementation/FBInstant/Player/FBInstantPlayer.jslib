var FBInstantPlayerLibrary = {

    $GetBuffer: function (str) {
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

    SaveUserData: function (key, json, callbackObj, callbackFunc, callbackId) {
        key = UTF8ToString(key);
        json = UTF8ToString(json);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        FBInstant.player.setDataAsync({ key: json })
            .then(() => {
                console.error("save user data ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: null, id: callbackId }));
            })
            .catch(err => {
                console.error("save user data error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: err.message, id: callbackId }));
            });
    },

    LoadUserData: function (key, callbackObj, callbackFunc, callbackId) {
        key = UTF8ToString(key);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        FBInstant.player.getDataAsync([key])
            .then(data => {
                console.error("load user data ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ data: data[key], error: null, id: callbackId }));
            })
            .catch(err => {
                console.error("load user data error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ data: null, error: err.message, id: callbackId }));
            });
    },

    player_canSubscribeBotAsync: function () {
        FBInstant.player.canSubscribeBotAsync()
            .then(function (can_subscribe) {
                console.log("can_subscribe=" + can_subscribe);
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_player_canSubscribeBotAsync", JSON.stringify(can_subscribe));
            }
            ).catch(function (e) {
                console.error("canSubscribeBotAsync|error|" + JSON.stringify(e));
            });
    },

    player_subscribeBotAsync: function () {
        FBInstant.player.subscribeBotAsync()
            .then(function () {
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_player_subscribeBotAsync_Success");
            }
            ).catch(function (e) {
                console.error("subscribeBotAsync|error|" + JSON.stringify(e));
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_player_subscribeBotAsync_Error", JSON.stringify(e));
            });
    },
};

autoAddDeps(FBInstantPlayerLibrary, "$GetBuffer");
mergeInto(LibraryManager.library, FBInstantPlayerLibrary);
