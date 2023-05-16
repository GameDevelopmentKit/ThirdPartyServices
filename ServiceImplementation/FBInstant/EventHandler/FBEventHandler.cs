namespace ServiceImplementation.FBInstant.EventHandler
{
    using ServiceImplementation.FBInstant.Notification;
    using ServiceImplementation.FBInstant.Tournament;
    using UnityEngine;
    using Zenject;

    public partial class FBEventHandler : MonoBehaviour
    {
        public static string callbackObj = "FBEventHandler";

        private FBTournament fbTournament;
        private FBNoti       fbNoti;

        [Inject]
        public void Construct(FBTournament fbTournament, FBNoti fbNoti)
        {
            this.fbTournament = fbTournament;
            this.fbNoti       = fbNoti;
        }

        #region Notification

        private void FacebookInstantSendInviteCallback() { this.fbNoti.FacebookInstantSendInviteCallback(); }

        private void FacebookInstantSendNotificationCallback() { this.fbNoti.FacebookInstantSendNotificationCallback(); }

        #endregion

        #region Tournament

        private void TournamentPostScoreAsyncCallback(string result) { this.fbTournament.TournamentPostScoreAsyncCallback(result); }

        private void GetTournamentAsyncCallback(string result) { this.fbTournament.GetTournamentAsyncCallback(result); }

        private void GetListTournamentAsyncCallback(string result) { this.fbTournament.GetListTournamentAsyncCallback(result); }

        private void OnJoinTournamentCallBack(string result) { this.fbTournament.OnJoinTournamentCallBack(result); }

        #endregion
    }
}