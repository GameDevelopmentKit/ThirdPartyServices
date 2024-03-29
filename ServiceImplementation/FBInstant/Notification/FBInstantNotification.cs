namespace ServiceImplementation.FBInstant.Notification
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GameFoundation.Scripts.Utilities.Extension;
    using Newtonsoft.Json;
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
        public static extern void fbinstant_inviteAlterAsync(string jsonStr, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        public static extern void fbinstant_notification(string action, string cta, string img, string content, string localizationJson, string template, string strategy, string notification, string callbackObj, string callbackFunctionName);

        #endregion

        #region Invite function

        public static void SendInvite(string text, Sprite image, Dictionary<string, object> @params = null)
        {
            @params          ??= new();
            @params["text"]  =   text;
            @params["image"] =   "data:image/png;base64," + Convert.ToBase64String(image.texture.EncodeToPNG());
            @params["sections"] = @params.GetOrDefault("sections") ?? new Dictionary<string, object>[]
            {
                new()
                {
                    { "sectionType", "USERS" },
                    { "maxResults", 10 },
                },
                new()
                {
                    { "sectionType", "GROUPS" },
                    { "maxResults", 5 },
                },
            };
            fbinstant_inviteAsync(JsonConvert.SerializeObject(@params), FBEventHandler.callbackObj, nameof(FacebookInstantSendInviteCallback));
        }

        public static void FacebookInstantSendInvite(Dictionary<string, object> p)
        {
            fbinstant_inviteAsync(JsonConvert.SerializeObject(p), FBEventHandler.callbackObj, nameof(FacebookInstantSendInviteCallback));
        }

        public static void FacebookInstantSendInviteAlter(Dictionary<string, object> p)
        {
            fbinstant_inviteAlterAsync(JsonConvert.SerializeObject(p), FBEventHandler.callbackObj, nameof(FacebookInstantSendInviteCallback));
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