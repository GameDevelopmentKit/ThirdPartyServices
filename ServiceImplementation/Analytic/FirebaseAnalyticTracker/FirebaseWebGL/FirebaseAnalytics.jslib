mergeInto(LibraryManager.library, {
    AnalyticsSetUserIdWeb: function (id) {
        var parsedId = UTF8ToString(id);
        firebaseSetUserId(analytics, parsedId);
    },

    AnalyticsSetUserPropertyWeb1: function (prop, value) {
        var parsedProp = UTF8ToString(prop);
        var parsedValue = UTF8ToString(value);

        var bundle = {};
        bundle[parsedProp] = parsedValue;

        firebaseSetUserProperties(analytics, bundle);
    },

    AnalyticsSetUserPropertyWeb2: function (properties) {
        var parsedProp = UTF8ToString(properties);
        firebaseSetUserProperties(analytics,JSON.parse(parsedProp));
    },

    AnalyticsLogEventWeb1: function (name, param, value) {
        var parsedName = UTF8ToString(name);
        var parsedParam = UTF8ToString(param);
        var parsedValue = UTF8ToString(value);
        console.log("AnalyticsLogEventWeb1: " + parsedName + "," + parsedParam + "," + "parsedValue");
        var bundle = {};
        bundle[parsedParam] = parsedValue;
        firebaseLogEvent(analytics,parsedName, bundle);
    },

    AnalyticsLogEventWeb2: function (name, parameters) {
        var parsedName = UTF8ToString(name);
        var parsedParams = UTF8ToString(parameters);
        console.log("AnalyticsLogEventWeb2: " + parsedName + "," + parsedParams);
        firebaseLogEvent(analytics,parsedName, JSON.parse(parsedParams));
    },

    AnalyticsLogEventWeb3: function (name) {
        var parsedName = UTF8ToString(name);
        console.log("AnalyticsLogEventWeb3: " + parsedName);
        firebaseLogEvent(analytics,parsedName);
    }

})
;
