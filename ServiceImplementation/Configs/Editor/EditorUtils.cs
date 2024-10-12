namespace ServiceImplementation.Configs.Editor
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.PackageManager;
    using UnityEngine;

    public static class EditorUtils
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

        // Modify the package in the manifest.json
        // add: true to add a package, false to remove a package
        // packageName: the package name to add or remove
        // packagePath: the package path to add, null to get the latest version
        public static void ModifyPackage(bool add, string packageName, string packagePath)
        {
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (File.Exists(manifestPath))
            {
                var manifestContent = File.ReadAllText(manifestPath);
                var manifestJson    = JObject.Parse(manifestContent);

                // Check if the package already exists
                var packageToken = manifestJson["dependencies"][packageName];

                if (add)
                {
                    if (packageToken == null)
                    {
                        // Add new package
                        manifestJson["dependencies"][packageName] = packagePath;
                        Debug.Log($"Package {packageName} added successfully.");
                    }
                    else
                    {
                        if (packageToken.ToString().Equals(packagePath)) Debug.LogWarning($"Package {packageName} already exists. No action taken.");
                        manifestJson["dependencies"][packageName] = packagePath;
                        Debug.LogWarning($"Update package path for {packageName}.");
                    }
                }
                else
                {
                    if (packageToken != null)
                    {
                        // Remove existing package
                        packageToken.Parent.Remove();
                        Debug.Log($"Package {packageName} removed successfully.");
                    }
                    else
                        Debug.LogWarning($"Package {packageName} does not exist. No action taken.");
                }

                // Write the updated JSON back to the file
                File.WriteAllText(manifestPath, manifestJson.ToString());
            }
            else
                Debug.LogError("manifest.json not found.");

            Client.Resolve();
        }
    }
}