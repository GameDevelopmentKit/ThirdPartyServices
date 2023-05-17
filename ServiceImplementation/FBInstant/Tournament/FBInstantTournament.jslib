var FBInstantTournamentLibrary = {

    tournamentPostScoreAsync: function (bestScore, callbackObj, callbackFunc) {
        bestScore = UTF8ToString(bestScore);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);
        FBInstant.tournament.postScoreAsync(bestScore)
            .then(() => {
                SendMessage(callbackObj, callbackFunc, JSON.stringify({score: bestScore}));
            });
    }
    ,

    getTournamentAsync: function (callbackObj, callbackFunc) {
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);
        FBInstant.getTournamentAsync()
            .then((tournament) => {
                SendMessage(callbackObj, callbackFunc, JSON.stringify({id: tournament.getID()}));
            });
    },

    getListTournamentAsync: function (callbackObj, callbackFunc) {
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);
        FBInstant.tournament.getTournamentsAsync()
            .then(tournaments => {
                // tournament list
                SendMessage(callbackObj, callbackFunc, JSON.stringify({listId: JSON.stringify(tournaments)}));
            });
    },

    joinTournamentAsync: function (tournamentID, callbackObj, callbackFunc) {
        tournamentID = UTF8ToString(tournamentID);
        callbackObj = UTF8ToString(callbackObj);
        callbackFunc = UTF8ToString(callbackFunc);
        FBInstant.tournament.joinAsync(tournamentID)
            .then(() => {
                SendMessage(callbackObj, callbackFunc, JSON.stringify({id: tournamentID}));
            });
    },
};

mergeInto(LibraryManager.library, FBInstantTournamentLibrary);
