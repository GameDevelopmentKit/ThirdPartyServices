using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class AppsflyerHelper
{
    private static string FileName = "AppsFlyerPurchaseConnector.asmdef";

    private static string Content = "{\n\t\"name\": \"AppsFlyerPurchaseConnector\"\n}\n";

    public static async Task SaveAssemblyDefinitionFile()
    {
        var appsflyerPath = Path.Combine(Application.dataPath, "AppsFlyer");

        if (!Directory.Exists(appsflyerPath))
        {
            await Task.Delay(1000);
            await SaveAssemblyDefinitionFile();

            return;
        }

        var fullPath = $"{appsflyerPath}/${FileName}";

        //write content to file
        if (!File.Exists(fullPath))
        {
            File.WriteAllText($"{appsflyerPath}/{FileName}", Content);
        }
    }

    public static async Task DownLoadPurchaseConnector(string url, string version)
    {
        var downloadURL     = url;
        var path            = Path.Combine(Application.temporaryCachePath, $"appsflyer-unity-purchase-connector-strict-mode-{version}.unitypackage.unitypackage");
        var downloadHandler = new DownloadHandlerFile(path);
        var webRequest      = new UnityWebRequest(downloadURL) { method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler };

        var operation = webRequest.SendWebRequest();

        await operation;

        if (webRequest.result == UnityWebRequest.Result.Success) AssetDatabase.ImportPackage(path, true);

        webRequest.Dispose();
    }

    public static void DeleteFolderWithMeta(string folderAssetPath)
    {
        if (Directory.Exists(folderAssetPath))
        {
            // Xóa folder qua AssetDatabase để Unity tự xử lý .meta
            FileUtil.DeleteFileOrDirectory(folderAssetPath);

            // Xóa file .meta đi kèm
            FileUtil.DeleteFileOrDirectory(folderAssetPath + ".meta");

            AssetDatabase.Refresh();
            Debug.Log($"Deleted folder and .meta: {folderAssetPath}");
        }
        else
        {
            Debug.LogWarning($"Folder not found: {folderAssetPath}");
        }
    }
}