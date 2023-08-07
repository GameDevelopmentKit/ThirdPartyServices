const FbInstantAdvertisementLibrary = {

    _showBannerAd: function (adId, callbackObj, callbackMethod, callbackId) {
        adId = UTF8ToString(adId);
        callbackObj = UTF8ToString(callbackObj);
        callbackMethod = UTF8ToString(callbackMethod);
        callbackId = UTF8ToString(callbackId);

        const sendMessage = (error = null) => SendMessage(
            callbackObj,
            callbackMethod,
            JSON.stringify({
                error: error ? JSON.stringify(error) : null,
                callbackId: callbackId,
            }),
        );

        FBInstant
            .loadBannerAdAsync(adId)
            .then(sendMessage)
            .catch(sendMessage);
    },

    _hideBannerAd: function (callbackObj, callbackMethod, callbackId) {
        callbackObj = UTF8ToString(callbackObj);
        callbackMethod = UTF8ToString(callbackMethod);
        callbackId = UTF8ToString(callbackId);

        const sendMessage = (error = null) => SendMessage(
            callbackObj,
            callbackMethod,
            JSON.stringify({
                error: error ? JSON.stringify(error) : null,
                callbackId: callbackId,
            }),
        );

        FBInstant
            .hideBannerAdAsync()
            .then(sendMessage)
            .catch(sendMessage);
    },

    _isInterstitialAdReady: function (adId) {
        return _isAdReady(adId);
    },

    _loadInterstitialAd: function (adId, callbackObj, callbackMethod, callbackId) {
        _loadAd(adId, true, callbackObj, callbackMethod, callbackId);
    },

    _showInterstitialAd: function (adId, callbackObj, callbackMethod, callbackId) {
        _showAd(adId, callbackObj, callbackMethod, callbackId);
    },

    _isRewardedAdReady: function (adId) {
        return _isAdReady(adId);
    },

    _loadRewardedAd: function (adId, callbackObj, callbackMethod, callbackId) {
        _loadAd(adId, false, callbackObj, callbackMethod, callbackId);
    },

    _showRewardedAd: function (adId, callbackObj, callbackMethod, callbackId) {
        _showAd(adId, callbackObj, callbackMethod, callbackId);
    },

    $_isAdReady: function (adId) {
        return !!(_getCache(UTF8ToString(adId)).loaded.length);
    },

    $_loadAd: function (adId, loadInter, callbackObj, callbackMethod, callbackId) {
        adId = UTF8ToString(adId);
        callbackObj = UTF8ToString(callbackObj);
        callbackMethod = UTF8ToString(callbackMethod);
        callbackId = UTF8ToString(callbackId);

        const sendMessage = (error = null) => SendMessage(
            callbackObj,
            callbackMethod,
            JSON.stringify({
                error: error ? JSON.stringify(error) : null,
                callbackId: callbackId,
            }),
        );

        const cache = _getCache(adId);

        const load = (ad) => ad.loadAsync()
            .then(() => {
                cache.loaded.push(ad);
                sendMessage();
            })
            .catch(error => {
                cache.loading.push(ad);
                sendMessage(error);
            });

        if (cache.loading.length) return load(cache.loading.shift());

        const promise = loadInter ? FBInstant.getInterstitialAdAsync(adId) : FBInstant.getRewardedVideoAsync(adId);

        promise.then(load).catch(sendMessage);
    },

    $_showAd: function (adId, callbackObj, callbackMethod, callbackId) {
        adId = UTF8ToString(adId);
        callbackObj = UTF8ToString(callbackObj);
        callbackMethod = UTF8ToString(callbackMethod);
        callbackId = UTF8ToString(callbackId);

        const sendMessage = (error = null) => SendMessage(
            callbackObj,
            callbackMethod,
            JSON.stringify({
                error: error ? JSON.stringify(error) : null,
                callbackId: callbackId,
            }),
        );

        const cache = _getCache(adId);

        if (!cache.loaded.length) return sendMessage(`Ad "${adId}" not loaded`);

        const ad = cache.loaded.shift();

        ad.showAsync()
            .then(sendMessage)
            .catch(error => {
                cache.loaded.push(ad);
                sendMessage(error);
            });
    },

    $_ads: {},

    $_getCache: function (adId) {
        adId = UTF8ToString(adId);
        if (!(adId in _ads)) {
            _ads[adId] = {
                loading: [],
                loaded: [],
            };
        }
        return _ads[adId];
    },
};

autoAddDeps(FbInstantAdvertisementLibrary, "$_isAdReady");
autoAddDeps(FbInstantAdvertisementLibrary, "$_loadAd");
autoAddDeps(FbInstantAdvertisementLibrary, "$_showAd");
autoAddDeps(FbInstantAdvertisementLibrary, "$_ads");
autoAddDeps(FbInstantAdvertisementLibrary, "$_getCache");
mergeInto(LibraryManager.library, FbInstantAdvertisementLibrary);