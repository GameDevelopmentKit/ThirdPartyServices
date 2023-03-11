namespace Core.AnalyticServices.Editor
{
    using global::Editor.GDKManager;
    using Models;
    using UnityEditor;
    using UnityEngine.UIElements;

    public class AnalyticConfigEditor : BaseGameConfigEditor<AnalyticConfig>
    {
        protected override string ConfigName { get; } = "AnalyticConfig";
        protected override string ConfigPath { get; } = "GameConfigs";
        public override VisualElement LoadView()
        {
            var analyticConfigTemplate = EditorGUIUtility.Load("Packages/com.gdk.3rd/Core/AnalyticServices/Editor/AnalyticConfigEditor.uxml") as VisualTreeAsset;

            if (analyticConfigTemplate == null) return this;
            var analyticConfigVisual = analyticConfigTemplate.CloneTree();
            analyticConfigVisual.Add(this.Config.CreateUIElementInspector());
            this.Add(analyticConfigVisual);

            return this;
        }
    }
}