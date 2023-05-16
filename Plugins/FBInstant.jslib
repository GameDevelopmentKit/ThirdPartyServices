var FBInstantLibrary = {
    $LoadedAds: {
        interstitialAd: {},
        rewardedAd: {},
    },

    $GetBuffer: function (str) {
        var size = lengthBytesUTF8(str) + 1;
        var buffer = _malloc(size);
        stringToUTF8(str, buffer, size);
        return buffer;
    },

    ShowBannerAd: function (placement) {
        placement = UTF8ToString(placement);
        FBInstant.loadBannerAdAsync(placement)
            .then(() => console.log("show banner ad ok"))
            .catch(err => console.error("show banner ad error: " + err.message))
    },

    HideBannerAd: function () {
        FBInstant.hideBannerAdAsync()
            .then(() => console.log("hide banner ad ok"))
            .catch(err => console.error("hide banner ad error: " + err.message))
    },

    IsInterstitialAdReady: function (placement) {
        placement = UTF8ToString(placement);
        return !!LoadedAds.interstitialAd[placement];
    },

    LoadInterstitialAd: function (placement, callbackObj, callbackFunc) {
        placement = UTF8ToString(placement);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        if (LoadedAds.interstitialAd[placement]) {
            console.log("preload interstitial ad ok");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            return;
        };

        const error = (err) => {
            console.error("preload interstitial ad error: " + err.message);
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: err.message }));
        }

        const success = (ad) => {
            ad.loadAsync().then(() => {
                LoadedAds.interstitialAd[placement] = ad
                console.log("preload interstitial ad ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            }).catch(error);
        }

        FBInstant.getInterstitialAdAsync(placement).then(success).catch(error);
    },

    ShowInterstitialAd: function (placement, callbackObj, callbackFunc) {
        placement = UTF8ToString(placement);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        var ad = LoadedAds.interstitialAd[placement];
        LoadedAds.interstitialAd[placement] = null;

        if (!ad) {
            console.log("interstitial ad not loaded");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: "interstitial ad not loaded" }));
            return;
        }

        ad.showAsync()
            .then(() => {
                console.error("show interstitial ad ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            })
            .catch(err => {
                console.error("show interstitial ad error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: err.message }));
            });
    },

    IsRewardedAdReady: function (placement) {
        placement = UTF8ToString(placement);

        return !!LoadedAds.rewardedAd[placement];
    },

    LoadRewardedAd: function (placement, callbackObj, callbackFunc) {
        placement = UTF8ToString(placement);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        if (LoadedAds.rewardedAd[placement]) {
            console.log("preload rewarded ad ok");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            return;
        };

        const error = (err) => {
            console.error("preload rewarded ad error: " + err.message);
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: err.message }));
        }

        const success = (ad) => {
            ad.loadAsync().then(() => {
                LoadedAds.rewardedAd[placement] = ad;
                console.log("preload rewarded ad ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            }).catch(error);
        }

        FBInstant.getRewardedVideoAsync(placement).then(success).catch(error);
    },

    ShowRewardedAd: function (placement, callbackObj, callbackFunc) {
        placement = UTF8ToString(placement);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        var ad = LoadedAds.rewardedAd[placement];
        LoadedAds.rewardedAd[placement] = null;

        if (!ad) {
            console.log("rewarded ad not loaded");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: "rewarded ad not loaded" }));
            return;
        }

        ad.showAsync()
            .then(() => {
                console.error("show rewarded ad ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            })
            .catch(err => {
                console.error("show rewarded ad error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: err.message }));
            });
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

    GetUserId: function () {
        return GetBuffer(FBInstant.player.getID());
    },

    GetUserName: function () {
        return GetBuffer(FBInstant.player.getName());
    },

    GetUserAvatar: function () {
        return GetBuffer(FBInstant.player.getPhoto());
    },

    PrintFloatArray: function (array, size) {
        for (var i = 0; i < size; i++)
            console.log(HEAPF32[(array >> 2) + i]);
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
            }).catch(function () {
                console.error("shareAsync|error|");
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
   
   // Tournament region 
     tournamentPostScoreAsync:function(bestScore, callbackObj, callbackFunc) {       
        FBInstant.tournament.postScoreAsync(bestScore)
         .then(function(){       
          SendMessage(callbackObj, callbackFunc, JSON.stringify({ score: bestScore }));      
          });                  
     }
    },
    
    getTournamentAsync:function(callbackObj, callbackFunc)
     {
     FBInstant.getTournamentAsync().then((tournament) => {
          SendMessage(callbackObj, callbackFunc, JSON.stringify({ id: tournament.getID() }));
     });
    },
    
    getListTournamentAsync:function(callbackObj, callbackFunc){
        FBInstant.tournament.getTournamentsAsync()
        .then(tournaments => {
        // tournament list
          SendMessage(callbackObj, callbackFunc, JSON.stringify({ listId:JSON.stringify(tournaments) }));
        });
    },
    
    joinTournamentAsync:function(tournamentID,callbackObj, callbackFunc)
    {
         FBInstant.tournament
         .joinAsync(tournamentID)
         .then(function() {
          SendMessage(callbackObj, callbackFunc, JSON.stringify({ id:tournamentID }));
         });
    },
};

autoAddDeps(FBInstantLibrary, '$LoadedAds');
mergeInto(LibraryManager.library, FBInstantLibrary);
