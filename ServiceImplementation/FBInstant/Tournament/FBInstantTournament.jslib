var FBInstantTournamentLibrary = {

    tournamentPostScoreAsync: function (bestScore, callbackObj, callbackFunc) {
        FBInstant.tournament.postScoreAsync(bestScore)
            .then(() => {
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ score: bestScore }));
            });
    }
    ,

    getTournamentAsync: function (callbackObj, callbackFunc) {
        FBInstant.getTournamentAsync()
            .then((tournament) => {
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ id: tournament.getID() }));
            });
    },

    getListTournamentAsync: function (callbackObj, callbackFunc) {
        FBInstant.tournament.getTournamentsAsync()
            .then(tournaments => {
                // tournament list
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ listId: JSON.stringify(tournaments) }));
            });
    },

    joinTournamentAsync: function (tournamentID, callbackObj, callbackFunc) {
        FBInstant.tournament.joinAsync(tournamentID)
            .then(() => {
                SendMessage(callbackObj, callbackFunc, JSON.stringify({ id: tournamentID }));
            });
    },
};

mergeInto(LibraryManager.library, FBInstantTournamentLibrary);
