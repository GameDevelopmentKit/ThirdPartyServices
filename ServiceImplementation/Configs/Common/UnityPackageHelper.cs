namespace ServiceImplementation.Configs.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Xml;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public static class UnityPackageHelper
    {
        [Serializable]
        private class GitHubRelease
        {
            public string tag_name;
        }

        /// <param name="apiEndpoint">Endpoint like: "https://api.github.com/repos/yandexmobile/yandex-ads-unity-plugin/releases/latest"</param>
        public static async UniTask<string> FetchLatestVersion(string apiEndpoint)
        {
            using var request = UnityWebRequest.Get(apiEndpoint);
            request.SetRequestHeader("User-Agent", "request");
            var operation = request.SendWebRequest();

            await UniTask.WaitUntil(() => operation.isDone);

            if (request.result == UnityWebRequest.Result.Success) return JsonUtility.FromJson<GitHubRelease>(request.downloadHandler.text).tag_name;

            Debug.LogError("Failed to fetch version: " + request.error);
            return "";
        }

        public static async UniTask DownloadThenImportPackage(string downloadUrl, string fileName)
        {
            #if UNITY_EDITOR
            var path            = Path.Combine(Application.temporaryCachePath, $"{fileName}.unitypackage");
            var downloadHandler = new DownloadHandlerFile(path);

            var webRequest = new UnityWebRequest(downloadUrl)
            {
                method          = UnityWebRequest.kHttpVerbGET,
                downloadHandler = downloadHandler,
            };

            await webRequest.SendWebRequest();

            #if UNITY_2020_1_OR_NEWER
            if (webRequest.result != UnityWebRequest.Result.Success)
                #else
            if (webRequest.isNetworkError || webRequest.isHttpError)
                #endif
            {
                Debug.LogError("onelog: Failed to download package: " + webRequest.error);
            }
            else
            {
                AssetDatabase.ImportPackage(path, false);
                AssetDatabase.Refresh();
            }

            webRequest.Dispose();
            #endif
        }

        public static async UniTaskVoid DownloadThenUnZip(string downloadUrl, string fileName, string unzipPath)
        {
            var path            = Path.Combine(Application.temporaryCachePath, $"{fileName}.zip");
            var downloadHandler = new DownloadHandlerFile(path);

            var webRequest = new UnityWebRequest(downloadUrl)
            {
                method          = UnityWebRequest.kHttpVerbGET,
                downloadHandler = downloadHandler,
            };

            var operation = webRequest.SendWebRequest();
            await operation;

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("onelog: Failed to download zip file: " + webRequest.error);
            }
            else
            {
                //unzip file to the specified path
                await UniTask.SwitchToMainThread();
                unzipPath = Path.Combine(Application.dataPath, unzipPath);
                ZipFile.ExtractToDirectory(path, unzipPath);
            }

            webRequest.Dispose();
        }

        public static bool DeleteFolderIfExists(string folderPath)
        {
            #if UNITY_EDITOR
            // Check if the folder exists
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                // Delete the folder
                AssetDatabase.DeleteAsset(folderPath);
                AssetDatabase.Refresh();

                Debug.Log($"Folder '{folderPath}' has been deleted.");

                return true;
            }
            #endif

            return false;
        }

        public static bool DeleteFileIfExists(string path)
        {
            #if UNITY_EDITOR
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();

                Debug.Log($"File '{path}' has been deleted.");

                return true;
            }
            #endif

            return false;
        }

        public static (string androidVersion, string iosVersion) ParseXmlFileGetPackageVersion(string path)
        {
            var androidPackages = new Dictionary<string, string>();
            var iosPods         = new Dictionary<string, string>();

            if (!File.Exists(path)) return ("", "");

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            var androidPackageNodes = xmlDoc.SelectNodes("/dependencies/androidPackages/androidPackage");
            if (androidPackageNodes != null)
                foreach (XmlNode node in androidPackageNodes)
                {
                    if (node.Attributes == null) continue;
                    var packageName = node.Attributes["spec"].Value;
                    androidPackages.Add(packageName.Split(':')[1], packageName.Split(':')[2]);
                }

            var iosPodNodes = xmlDoc.SelectNodes("/dependencies/iosPods/iosPod");
            if (iosPodNodes != null)
                foreach (XmlNode node in iosPodNodes)
                {
                    if (node.Attributes == null) continue;
                    var podName    = node.Attributes["name"].Value;
                    var podVersion = node.Attributes["version"].Value;
                    iosPods.Add(podName, podVersion);
                }

            return (androidPackages.Values.FirstOrDefault(), iosPods.Values.FirstOrDefault());
        }

        public static void CreateFileWithContent(string filePath, string content)
        {
            #if UNITY_EDITOR
            if (!File.Exists(filePath))
            {
                var dir = Path.GetDirectoryName(filePath) ?? ".";
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh();
            #endif
        }

        public static void CopyFile(string filePath, string sourcePath)
        {
            #if UNITY_EDITOR
            CreateFileWithContent(filePath, File.ReadAllText(sourcePath));
            #endif
        }
    }
}