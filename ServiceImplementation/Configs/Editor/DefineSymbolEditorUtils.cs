namespace ServiceImplementation.Configs.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;

    public static class DefineSymbolEditorUtils
    {
        private const string Delemiter = ";";

        public static void SetDefineSymbol(string symbol, bool isAdd)
        {
            SetBuildTargetDefineSymbol(NamedBuildTarget.Android, symbol, isAdd);
            SetBuildTargetDefineSymbol(NamedBuildTarget.iOS, symbol, isAdd);
            SetBuildTargetDefineSymbol(NamedBuildTarget.WebGL, symbol, isAdd);
            SetBuildTargetDefineSymbol(NamedBuildTarget.Standalone, symbol, isAdd);
            SetBuildTargetDefineSymbol(NamedBuildTarget.Server, symbol, isAdd);
        }

        private static void SetBuildTargetDefineSymbol(NamedBuildTarget buildTarget, string symbol, bool isAdd)
        {
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget).Split(Delemiter).ToList();
            if (isAdd)
            {
                if (defineSymbols.Contains(symbol)) return;
                defineSymbols.Add(symbol);
            }
            else
            {
                if (!defineSymbols.Contains(symbol)) return;
                defineSymbols.Remove(symbol);
            }

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(Delemiter, defineSymbols));
        }
    }
}