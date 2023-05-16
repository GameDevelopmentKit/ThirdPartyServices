namespace ServiceImplementation.FBInstant.EventHandler
{
    using ServiceImplementation.FBInstant.Notification;
    using ServiceImplementation.FBInstant.Tournament;
    using UnityEngine;
    using Zenject;

    public partial class FBEventHandler : MonoBehaviour
    {
        public static string callbackObj = "FBEventHandler";

        private FBInstantTournament   fbTournament;
        private FBInstantNotification fbNotification;

        [Inject]
        public void Construct(FBInstantTournament fbTournament, FBInstantNotification fbNotification)
        {
            this.fbTournament   = fbTournament;
            this.fbNotification = fbNotification;
        }

        #region Notification

        private void FacebookInstantSendInviteCallback() { this.fbNotification.FacebookInstantSendInviteCallback(); }

        private void FacebookInstantSendNotificationCallback() { this.fbNotification.FacebookInstantSendNotificationCallback(); }

        #endregion

        #region Tournament

        private void TournamentPostScoreAsyncCallback(string result) { this.fbTournament.TournamentPostScoreAsyncCallback(result); }

        private void GetTournamentAsyncCallback(string result) { this.fbTournament.GetTournamentAsyncCallback(result); }

        private void GetListTournamentAsyncCallback(string result) { this.fbTournament.GetListTournamentAsyncCallback(result); }

        private void OnJoinTournamentCallBack(string result) { this.fbTournament.OnJoinTournamentCallBack(result); }

        #endregion
    }
}