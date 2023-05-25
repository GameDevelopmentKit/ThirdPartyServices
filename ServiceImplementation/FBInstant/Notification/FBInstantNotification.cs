using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceImplementation.FBInstant.Notification
{
    using System.Runtime.InteropServices;
    using ServiceImplementation.FBInstant.EventHandler;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// External interface for FBInstant Notification with Message and Facebook Notification
    /// </summary>
    public class FBInstantNotification
    {
        #region javascript

        [DllImport("__Internal")]
        public static extern void fbinstant_inviteAsync(string jsonStr, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        public static extern void fbinstant_notification(string action, string cta, string img, string content, string localizationJson,
            string template, string strategy, string notification, string callbackObj, string callbackFunctionName);

        #endregion

        #region Invite function

        public static void FacebookInstantSendInvite(Dictionary<string, object> p)
        {
            fbinstant_inviteAsync(JsonConvert.SerializeObject(p), FBEventHandler.callbackObj, nameof(FacebookInstantSendInviteCallback));
        }

        public void FacebookInstantSendInviteCallback()
        {
            // Invite sent
        }

        #endregion

        #region Notification function

        public static void SendNotification(string action, string cta, Image img, string content, string localizationJson,
            string template, string strategy, string notification)
        {
            var imgBase64 = img.sprite.texture.EncodeToPNG();
            fbinstant_notification(action, cta, imgBase64.ToString(), content, localizationJson, template, strategy, notification, FBEventHandler.callbackObj,
                nameof(FacebookInstantSendNotificationCallback));
        }

        public void FacebookInstantSendNotificationCallback()
        {
            // Notification sent
        }

        #endregion
    }
}