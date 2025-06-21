using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using UnityEngine;

public static class ApplovinDownloadHelper
{
    private static string UrlGithub = "https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases?page=";

    public static async Task<string> GetDownloadPackage(string version = "8.2.0")
    {
        var pluginVersion  = "8.2.0";
        var androidversion = "13.2.0";
        var iosversion     = "13.2.0";
        var result         = "";

        var config  = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        var firstPage = await context.OpenAsync($"{UrlGithub}1");
        var maxPage   = GetMaxPage(firstPage);

        for (var i = 1; i <= maxPage; i++)
        {
            Debug.Log($"Finding version {version} in {i} / {maxPage}");
            var pageUrl  = $"{UrlGithub}{i}";
            var document = await context.OpenAsync(pageUrl);
            var html     = document.ToHtml();

            var list = await ExtractAppLovinVersions(html);

            foreach (var item in list)
            {
                if (item.PluginVersion == version)
                {
                    pluginVersion  = item.PluginVersion;
                    androidversion = item.AndroidVersion ?? "unknown";
                    iosversion     = item.IOSVersion ?? "unknown";

                    result = $"https://artifacts.applovin.com/unity/com/applovin/applovin-sdk/AppLovin-MAX-Unity-Plugin-{pluginVersion}-Android-{androidversion}-iOS-{iosversion}.unitypackage";

                    return result;
                }
            }
        }

        return result;
    }

    private static int GetMaxPage(IDocument document)
    {
        int maxPage = 1;
        var node    = document.QuerySelector(".paginate-container.d-none.d-sm-flex.flex-sm-justify-center");

        if (node != null)
        {
            var links = node.QuerySelectorAll("a");

            foreach (var link in links)
            {
                var text = link.TextContent.Trim();

                if (int.TryParse(text, out int pageNum) && pageNum > maxPage)
                {
                    maxPage = pageNum;
                }
            }
        }

        return maxPage;
    }

    public static async Task<List<(string PluginVersion, string AndroidVersion, string IOSVersion)>> ExtractAppLovinVersions(string html)
    {
        var result = new List<(string PluginVersion, string AndroidVersion, string IOSVersion)>();

        var config   = Configuration.Default;
        var context  = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html));

        var releaseBoxes = document.QuerySelectorAll(".Box-body");

        foreach (var box in releaseBoxes)
        {
            var    pluginVersion  = box.QuerySelector(".f1.text-bold a")?.TextContent.Trim();
            string androidVersion = null;
            string iosVersion     = null;

            foreach (var link in box.QuerySelectorAll("a"))
            {
                var href = link.GetAttribute("href");

                if (href == null) continue;

                if (href.Contains("AppLovin-MAX-SDK-Android"))
                    androidVersion = ExtractVersionFromUrl(href);
                else if (href.Contains("AppLovin-MAX-SDK-iOS"))
                    iosVersion = ExtractVersionFromUrl(href);
            }

            if (!string.IsNullOrEmpty(pluginVersion))
                result.Add((pluginVersion, androidVersion, iosVersion));
        }

        return result;
    }

    private static string ExtractVersionFromUrl(string url)
    {
        var match = Regex.Match(url, @"release[_\-](\d+[_\-]\d+[_\-]?\d*)");

        return match.Success ? match.Groups[1].Value.Replace("_", ".").Replace("-", ".") : null;
    }
}