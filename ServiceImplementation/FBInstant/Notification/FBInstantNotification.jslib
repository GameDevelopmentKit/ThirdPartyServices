var FBInstantNotificationLibrary = {
    // Invite
    fbinstant_inviteAsync: function (jsonStr, callbackObj, callbackFunctionName) {
                var param = JSON.parse(UTF8ToString(jsonStr));
                FBInstant.inviteAsync(param)
                            .then(function () {
                                SendMessage(callbackObj, callbackFunctionName);
                            }).catch(function (error) {
                                console.error("inviteAsync|error|"  + JSON.stringify(error));
                            });
            },

    // Send notification
    fbinstant_notification: function (action, cta, img, content, localizationJson, template, strategy, notification, callbackObj, callbackFunctionName) {
        var localizations = JSON.parse(UTF8ToString(localizationJson));
        FBInstant.updateAsync({
            action: action,
            cta: cta,
            image: img,
            text: {
                default: content,
                localizations: localizations
            },
            template: template,
            data: {myReplayData: '...'},
            strategy: strategy,
            notification: notification,
        }).then(function () {
            SendMessage(callbackObj, callbackFunctionName);
            // closes the game after the update is posted.
            FBInstant.quit();
        });
    },
};

mergeInto(LibraryManager.library, FBInstantNotificationLibrary);
