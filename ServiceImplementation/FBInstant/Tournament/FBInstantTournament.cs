namespace ServiceImplementation.FBInstant.Tournament
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FBInstantTournament : MonoBehaviour
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

        public void TournamentPostScoreAsync(int score) { tournamentPostScoreAsync(score, this.gameObject.name, nameof(this.TournamentPostScoreAsyncCallback)); }

        public void GetTournamentAsync(Action<int> onComplete)
        {
            this.onCompleteGetTournament = onComplete;
            getTournamentAsync(this.gameObject.name, nameof(this.GetTournamentAsyncCallback));
        }

        public void GetListTournamentAsync(Action<List<int>> onComplete)
        {
            this.onCompleteGetListTournament = onComplete;
            getListTournamentAsync(this.gameObject.name, nameof(this.GetListTournamentAsyncCallback));
        }

        public void JoinTournamentAsync(string tournamentID, Action<int> onComplete)
        {
            this.onJoinTournamentCallBack = onComplete;
            joinTournamentAsync(tournamentID, this.gameObject.name, nameof(this.OnJoinTournamentCallBack));
        }

        private void TournamentPostScoreAsyncCallback(string result) { }

        private void GetTournamentAsyncCallback(string result)
        {
            var @params      = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var tournamentId = @params["id"];

            if (int.TryParse(tournamentId, out var outId))
            {
                this.onCompleteGetTournament?.Invoke(outId);
            }
        }

        private void GetListTournamentAsyncCallback(string result)
        {
            var @params          = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var listIdJsonString = @params["listId"];

            if (string.IsNullOrEmpty(listIdJsonString)) return;
            var listId = JsonConvert.DeserializeObject<List<int>>(listIdJsonString);
            this.onCompleteGetListTournament?.Invoke(listId);
        }

        private void OnJoinTournamentCallBack(string result)
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