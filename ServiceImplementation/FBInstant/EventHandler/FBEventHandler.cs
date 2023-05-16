namespace ServiceImplementation.FBInstant.EventHandler
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using ServiceImplementation.FBInstant.Tournament;
    using UnityEngine;
    using Zenject;

    public class FBEventHandler : MonoBehaviour
    {
        public static string callbackObj = "FBEventHandler";

        private FBTournament fbTournament;

        [Inject]
        public void Construct(FBTournament fbTournament) { this.fbTournament = fbTournament; }

        #region Notification

        private void FacebookInstantSendInviteCallback() { Debug.Log("Invite sent"); }

        private void FacebookInstantSendNotificationCallback() { Debug.Log("Notification sent"); }

        #endregion

        #region Tournament

        private void TournamentPostScoreAsyncCallback(string result) { }

        private void GetTournamentAsyncCallback(string result)
        {
            var @params      = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var tournamentId = @params["id"];

            if (int.TryParse(tournamentId, out var outId))
            {
                this.fbTournament.onCompleteGetTournament?.Invoke(outId);
            }
        }

        private void GetListTournamentAsyncCallback(string result)
        {
            var @params          = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var listIdJsonString = @params["listId"];

            if (string.IsNullOrEmpty(listIdJsonString)) return;
            var listId = JsonConvert.DeserializeObject<List<int>>(listIdJsonString);
            this.fbTournament.onCompleteGetListTournament?.Invoke(listId);
        }

        private void OnJoinTournamentCallBack(string result)
        {
            var @params      = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            var tournamentId = @params["id"];

            if (string.IsNullOrEmpty(tournamentId)) return;

            if (int.TryParse(tournamentId, out var outId))
            {
                this.fbTournament.onJoinTournamentCallBack?.Invoke(outId);
            }
        }

        #endregion
    }
}