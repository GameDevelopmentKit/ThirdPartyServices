namespace ThirdParty.WebGL.Editor
{
    using System;
    using System.Linq;
    using Core.AdsServices.Native;
    using ServiceImplementation.Configs;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class NativePlacementAttributeDrawer : OdinAttributeDrawer<NativePlacementAttribute>
    {
        private string[] options = Array.Empty<string>();

        protected override void Initialize()
        {
            base.Initialize();
            this.options = Resources.Load<ThirdPartiesConfig>(ThirdPartiesConfig.ResourcePath).AdSettings.AdMob.NativeAdIds.Select(pair => pair.Key.Name).ToArray();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.Property.ValueEntry.TypeOfValue != typeof(string))
            {
                EditorGUILayout.HelpBox(new GUIContent($"{this.Attribute.GetType().FullName} is not support type: {this.Property.ValueEntry.TypeOfValue.FullName}"));

                return;
            }
            var propValue = (string)this.Property.ValueEntry.WeakSmartValue;
            var index     = 0;

            if (this.options.Length == 0)
            {
                EditorGUILayout.HelpBox("Not found Placement, fill it pls!", MessageType.Error);

                return;
            }

            if (string.IsNullOrEmpty(propValue) || !this.options.Contains(propValue))
            {
                this.Property.ValueEntry.WeakSmartValue = this.options[0];
            }
            else
            {
                index += this.options.TakeWhile(opt => !propValue.Equals(opt)).Count();
            }

            if (index > this.options.Length)
            {
                EditorGUILayout.HelpBox(new GUIContent($"Index {index} out of range!"));

                return;
            }
            var selectedIndex = EditorGUILayout.Popup(this.Property.NiceName, index, this.options);
            this.Property.ValueEntry.WeakSmartValue = this.options[selectedIndex];
        }
    }
}