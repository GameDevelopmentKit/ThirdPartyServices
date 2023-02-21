mergeInto(LibraryManager.library, {
    AnalyticsSetUserIdWeb : function(id){
        var parsedId = UTF8ToString(id);
        firebase.analytics().setUserId(parsedId);
    },
    
    AnalyticsSetUserPropertyWeb1 : function(prop, value){
        var parsedProp = UTF8ToString(prop);
        var parsedValue = UTF8ToString(value);
        
        var bundle = {};
        bundle[parsedProp] = parsedValue;
        
        firebase.analytics().setUserProperties(bundle);
    },
    
    AnalyticsSetUserPropertyWeb2 : function(properties){
         var parsedProp = UTF8ToString(properties);
           
         firebase.analytics().setUserProperties(JSON.parse(parsedProp));
    },
    
    AnalyticsLogEventWeb1 : function(name, param, value){
         var parsedName = UTF8ToString(name);
         var parsedParam = UTF8ToString(param);
         var parsedValue = UTF8ToString(value);
         console.log("AnalyticsLogEventWeb1: " + parsedName + "," + parsedParam + "," + "parsedValue");
          var bundle = {};
          bundle[parsedParam] = parsedValue;
         firebase.analytics().logEvent(parsedName,bundle);
    },
    
    AnalyticsLogEventWeb2 : function(name, parameters){
         var parsedName = UTF8ToString(name);
         var parsedParams = UTF8ToString(parameters);
         console.log("AnalyticsLogEventWeb2: " + parsedName + "," + parsedParams);    
         firebase.analytics().logEvent(parsedName, JSON.parse(parsedParams));
    },
    
    AnalyticsLogEventWeb3 : function(name){
          var parsedName = UTF8ToString(name);       
          console.log("AnalyticsLogEventWeb3: " + parsedName);                         
          firebase.analytics().logEvent(parsedName);   
    }

})
;
