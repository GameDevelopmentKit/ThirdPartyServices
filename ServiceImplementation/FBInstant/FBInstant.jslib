var FBInstantLibrary = {

    PrintFloatArray: function (array, size) {
        for (var i = 0; i < size; i++)
            console.log(HEAPF32[(array >> 2) + i]);
    },

    context_chooseAsync: function (jsonStr) {
        var param = JSON.parse(UTF8ToString(jsonStr));

        FBInstant.context.chooseAsync(param).then(function () {
            FBInstant.logEvent("player_chooose_context", 1, FBInstant.getEntryPointData());
            gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_context_chooseAsync");
        });
    },

    context_getPlayersAsync: function () {
        FBInstant.context.getPlayersAsync().then(function (playerList) {
            var data = [];
            for (var i = 0; i < playerList.length; i++) {
                var player = playerList[i];
                var item = {
                    id: player.getID(),
                    name: player.getName(),
                    photo: player.getPhoto(),
                };
                data.push(item);
            }

            gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_context_getPlayersAsync", JSON.stringify(data));
        })
            .catch(function (err) {
                console.error('context_getPlayersAsync|' + JSON.stringify(err));
            });
    },

    context_getType: function () {
        var val = FBInstant.context.getType();
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    context_getID: function () {
        var val = FBInstant.context.getID();
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    fbinstant_getSupportedAPIs: function () {
        var val = JSON.stringify(FBInstant.getSupportedAPIs());
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    fbinstant_switchGameAsync: function (appId, entryPointDataJsonStr) {
        appId = UTF8ToString(appId);
        var entryPointData = JSON.parse(UTF8ToString(entryPointDataJsonStr));

        FBInstant.switchGameAsync(appId, entryPointData)
            .catch(function (e) {
                console.error("switchGameAsync|error|" + JSON.stringify(e));
            });
    },

    fbinstant_updateAsync: function (jsonStr) {
        var param = JSON.parse(UTF8ToString(jsonStr));
        FBInstant.updateAsync(param)
            .then(function () { })
            .catch(function (err) {
                console.error('updateAsync|' + JSON.stringify(err));
            });
    },

    fbinstant_shareAsync: function (jsonStr) {
        var param = JSON.parse(UTF8ToString(jsonStr));

        FBInstant.shareAsync(param)
            .then(function () {
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_fbinstant_shareAsync");
            }).catch(function (error) {
                console.error("shareAsync|error|"+ JSON.stringify(error));
            });
    },
    leaderboard_setScoreAsync: function (keyName, extraDataJsonStr, score) {
        keyName = UTF8ToString(keyName);
        extraDataJsonStr = UTF8ToString(extraDataJsonStr);

        FBInstant.getLeaderboardAsync(keyName)
            .then(function (leaderboard) {
                return leaderboard.setScoreAsync(score, extraDataJsonStr);
            })
            .then(function (entry) {
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_leaderboard_setScoreAsync");
            })
            .catch(function (error) {
                console.error("leaderboard|setScoreAsync|error|" + JSON.stringify(error));
            });
    },

    leaderboard_getPlayerEntryAsync: function (keyName) {
        keyName = UTF8ToString(keyName);

        FBInstant.getLeaderboardAsync(keyName)
            .then(function (leaderboard) {
                return leaderboard.getPlayerEntryAsync();
            })
            .then(function (entry) {
                if (entry) {
                    gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_leaderboard_getPlayerEntryAsync", JSON.stringify({ "rank": entry.getRank(), "score": entry.getScore() }));
                } else {
                    gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_leaderboard_getPlayerEntryAsync", JSON.stringify({ "rank": -1, "score": 0 }));
                }
            })
            .catch(function (error) {
                console.error("leaderboard|getPlayerEntryAsync|error|" + JSON.stringify(error));
            });
    },

    leaderboard_getEntriesAsync: function (keyName, limit) {
        keyName = UTF8ToString(keyName);

        FBInstant.getLeaderboardAsync(keyName)
            .then(function (leaderboard) {
                return leaderboard.getEntriesAsync(Math.min(100, limit), 0);
            })
            .then(function (entries) {
                var data = [];
                for (var i = 0; i < entries.length; i++) {
                    var element = entries[i];
                    var item = {
                        avatarUrl: element.getPlayer().getPhoto(),
                        nickName: element.getPlayer().getName(),
                        score: element.getScore(),
                        extraData: element.getExtraData() ? JSON.parse(element.getExtraData()) : null,
                    };
                    data.push(item);
                }

                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_leaderboard_getEntriesAsync", JSON.stringify(data));
            });
    },

    leaderboard_getConnectedPlayerEntriesAsync: function (keyName, limit) {
        keyName = UTF8ToString(keyName);

        FBInstant.getLeaderboardAsync(keyName)
            .then(function (leaderboard) {
                return leaderboard.getConnectedPlayerEntriesAsync(Math.min(100, limit), 0);
            })
            .then(function (entries) {
                var data = [];
                for (var i = 0; i < entries.length; i++) {
                    var element = entries[i];
                    var item = {
                        avatarUrl: element.getPlayer().getPhoto(),
                        nickName: element.getPlayer().getName(),
                        score: element.getScore(),
                        extraData: element.getExtraData() ? JSON.parse(element.getExtraData()) : null,
                    };
                    data.push(item);
                }

                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_leaderboard_getConnectedPlayerEntriesAsync", JSON.stringify(data));
            });
    },

    payments_onReady: function () {
        FBInstant.payments.onReady(function () {
            console.log('Payments Ready!');
            gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_payments_onReady_Callback");
        });
    },

    payments_getCatalogAsync: function () {
        FBInstant.payments.getCatalogAsync().then(function (catalog) {
            console.log(JSON.stringify(catalog));
            gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_getCatalogAsync_Callback", JSON.stringify(catalog));
        }).catch(function (err) {
            console.error("payments_getCatalogAsync:" + JSON.stringify(err));
            gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_getCatalogAsync_Callback_Error", JSON.stringify(err));
        });
    },

    payments_purchaseAsync: function (purchaseConfigJsonStr) {
        purchaseConfigJsonStr = UTF8ToString(purchaseConfigJsonStr);

        var purchaseConfig = JSON.parse(purchaseConfigJsonStr);

        FBInstant.payments.purchaseAsync(purchaseConfig)
            .then(function (purchase) {
                console.log(JSON.stringify(purchase));
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_purchaseAsync_Callback", JSON.stringify(purchase));
            }).catch(function (err) {
                console.error("payments_purchaseAsync:" + JSON.stringify(err));
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_purchaseAsync_Callback_Error", JSON.stringify(err));
            });
    },

    payments_getPurchasesAsync: function () {
        FBInstant.payments.getPurchasesAsync()
            .then(function (purchases) {
                console.log(JSON.stringify(purchases));
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_getPurchasesAsync_Callback", JSON.stringify(purchases));
            }).catch(function (err) {
                console.error("payments_getPurchasesAsync:" + JSON.stringify(err));
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_getPurchasesAsync_Callback_Error", JSON.stringify(err));
            });
    },

    payments_consumePurchaseAsync: function (purchaseToken) {
        purchaseToken = UTF8ToString(purchaseToken);

        FBInstant.payments.consumePurchaseAsync(purchaseToken)
            .then(function () {
                console.log("Purchase successfully consumed!");
                // Game should now provision the product to the player
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_consumePurchaseAsync_Callback");
            }).catch(function (err) {
                console.error("payments_consumePurchaseAsync:" + JSON.stringify(err));
                gameInstance.SendMessage("__IGEXPORTER__", "Promise_on_consumePurchaseAsync_Callback_Error", JSON.stringify(err));
            });
    },

    getEntryPointData: function () {
        const entryPointData = FBInstant.getEntryPointData();
        if (!entryPointData) {
            return "";
        }

        var val = JSON.stringify(entryPointData);
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    getLocale: function () {
        var val = FBInstant.getLocale();
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    quit: function () {
        FBInstant.quit();
    },

    logEvent: function (eventName, valueToSum, jsonStr) {
        eventName = UTF8ToString(eventName);
        jsonStr = UTF8ToString(jsonStr);
        var parameters = {};
        if (jsonStr) {
            parameters = JSON.parse(jsonStr);
        }

        FBInstant.logEvent(eventName, valueToSum, parameters);
    },

    getPlatform: function () {
        var val = FBInstant.getPlatform();
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    getSDKVersion: function () {
        var val = FBInstant.getSDKVersion();
        var bufferSize = lengthBytesUTF8(val) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(val, buffer, bufferSize);
        return buffer;
    },

    //this require set attribute preserveDrawingBuffer to true, default is false.
    htmlcanvas_toImageBase64: function (type, encoderOptions) {
        type = UTF8ToString(type);

        var canvas = document.getElementById("#canvas");
        var base64 = canvas.toDataURL(type, encoderOptions);

        var bufferSize = lengthBytesUTF8(base64) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(base64, buffer, bufferSize);
        return buffer;
    },
};

mergeInto(LibraryManager.library, FBInstantLibrary);
