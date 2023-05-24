var FBInstantAdsLibrary = {

    $LoadedAds: {},

    ShowBannerAd: function (adId) {
        adId = UTF8ToString(adId);
        FBInstant.loadBannerAdAsync(adId)
            .then(() => console.log("show banner ad ok"))
            .catch(err => console.error("show banner ad error: " + err.message))
    },

    HideBannerAd: function () {
        FBInstant.hideBannerAdAsync()
            .then(() => console.log("hide banner ad ok"))
            .catch(err => console.error("hide banner ad error: " + err.message))
    },

    IsInterstitialAdReady: function () {
        return !!LoadedAds.interstitialAd;
    },

    LoadInterstitialAd: function (adId, callbackObj, callbackFunc) {
        adId = UTF8ToString(adId);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        if (LoadedAds.interstitialAd) {
            console.log("preload interstitial ad ok");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: null }));
            return;
        }

        FBInstant.getInterstitialAdAsync(adId)
            .then(ad => ad.loadAsync()
                .then(() => {
                    LoadedAds.interstitialAd = ad
                    console.log("preload interstitial ad ok");
                    SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: null }));
                }))
            .catch((err) => {
                console.error("preload interstitial ad error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: err.message }))
            });
    },

    ShowInterstitialAd: function (placement, callbackObj, callbackFunc) {
        placement = UTF8ToString(placement);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        var ad = LoadedAds.interstitialAd;
        LoadedAds.interstitialAd = null;

        if (!ad) {
            console.error("interstitial ad not loaded");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: "interstitial ad not loaded" }));
            return;
        }

        ad.showAsync()
            .then(() => {
                console.log("show interstitial ad ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            })
            .catch(err => {
                console.error("show interstitial ad error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: err.message }));
            });
    },

    IsRewardedAdReady: function () {
        return !!LoadedAds.rewardedAd;
    },

    LoadRewardedAd: function (adId, callbackObj, callbackFunc) {
        adId = UTF8ToString(adId);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        if (LoadedAds.rewardedAd) {
            console.log("preload rewarded ad ok");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: null }));
            return;
        }

        FBInstant.getRewardedVideoAsync(placement)
            .then((ad) => ad.loadAsync()
                .then(() => {
                    LoadedAds.rewardedAd = ad;
                    console.log("preload rewarded ad ok");
                    SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: null }));
                }))
            .catch((err) => {
                console.error("preload rewarded ad error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ error: err.message }));
            });
    },

    ShowRewardedAd: function (placement, callbackObj, callbackFunc) {
        placement = UTF8ToString(placement);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);

        var ad = LoadedAds.rewardedAd;
        LoadedAds.rewardedAd = null;

        if (!ad) {
            console.error("rewarded ad not loaded");
            SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: "rewarded ad not loaded" }));
            return;
        }

        ad.showAsync()
            .then(() => {
                console.log("show rewarded ad ok");
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: null }));
            })
            .catch(err => {
                console.error("show rewarded ad error: " + err.message);
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ place: placement, error: err.message }));
            });
    },
};

autoAddDeps(FBInstantAdsLibrary, "$LoadedAds");
mergeInto(LibraryManager.library, FBInstantAdsLibrary);
