var FBInstantAdsLibrary = {

    $LoadedAds: {
        interstitialAd: {},
        rewardedAd: {},
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
