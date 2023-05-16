namespace ServiceImplementation.FBInstant.Tournament
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using ServiceImplementation.FBInstant.EventHandler;
    using UnityEngine;

    public class FBInstantTournament
    {
        #region Javascript

        [DllImport("__Internal")]
        public static extern string tournamentPostScoreAsync(int bestScore, string callBackObj, string callBackMethod);

        [DllImport("__Internal")]
        public static extern string getTournamentAsync(string callBackObj, string callBackMethod);

        [DllImport("__Internal")]
        public static extern string getListTournamentAsync(string callBackObj, string callBackMethod);

        [DllImport("__Internal")]
        public static extern string joinTournamentAsync(string tournamentID, string callBackObj, string callBackMethod);

        #endregion

        private Action<int>       onCompleteGetTournament, onJoinTournamentCallBack;
        private Action<List<int>> onCompleteGetListTournament;

        public void TournamentPostScoreAsync(int score) { tournamentPostScoreAsync(score, FBEventHandler.callbackObj, nameof(this.TournamentPostScoreAsyncCallback)); }

        public void GetTournamentAsync(Action<int> onComplete)
        {
            this.onCompleteGetTournament = onComplete;
            getTournamentAsync(FBEventHandler.callbackObj, nameof(this.GetTournamentAsyncCallback));
        }

        public void GetListTournamentAsync(Action<List<int>> onComplete)
        {
            this.onCompleteGetListTournament = onComplete;
            getListTournamentAsync(FBEventHandler.callbackObj, nameof(this.GetListTournamentAsyncCallback));
        }

        public void JoinTournamentAsync(string tournamentID, Action<int> onComplete)
        {
            this.onJoinTournamentCallBack = onComplete;
            joinTournamentAsync(tournamentID, FBEventHandler.callbackObj, nameof(this.OnJoinTournamentCallBack));
        }

        public void TournamentPostScoreAsyncCallback(string result) { }

        public void GetTournamentAsyncCallback(string result)
        {
            var @params      = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var tournamentId = @params["id"];

            if (int.TryParse(tournamentId, out var outId))
            {
                this.onCompleteGetTournament?.Invoke(outId);
            }
        }

        public void GetListTournamentAsyncCallback(string result)
        {
            var @params          = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var listIdJsonString = @params["listId"];

            if (string.IsNullOrEmpty(listIdJsonString)) return;
            var listId = JsonConvert.DeserializeObject<List<int>>(listIdJsonString);
            this.onCompleteGetListTournament?.Invoke(listId);
        }

        public void OnJoinTournamentCallBack(string result)
        {
            var @params      = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var tournamentId = @params["id"];

            if (string.IsNullOrEmpty(tournamentId)) return;

            if (int.TryParse(tournamentId, out var outId))
            {
                this.onJoinTournamentCallBack?.Invoke(outId);
            }
        }
    }
}