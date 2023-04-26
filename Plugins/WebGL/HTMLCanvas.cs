namespace Plugins.WebGL
{
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class HTMLCanvas
    {

        [DllImport("__Internal")]
        public static extern string htmlcanvas_toImageBase64(string type, int encoderOptions);

        /// <summary>
        /// Reference: https://developer.mozilla.org/zh-CN/docs/Web/API/HTMLCanvasElement/toDataURL
        /// Caution: When call this function it will take a little long time
        /// </summary>
        /// <param name="type"></param>
        /// <param name="encoderOptions">0-1: quality of the image</param>
        /// <returns></returns>
        public static string ToImageBase64(string type, int encoderOptions)
        {
            string base64 = htmlcanvas_toImageBase64(type, encoderOptions);
            return base64;
        }

        /// <summary>
        /// Use this method first
        /// </summary>
        /// <returns></returns>
        public static string GetScreenShotBase64()
        {
            Texture2D tex = new Texture2D(Screen.width, Screen.height);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();
            byte[] img = tex.EncodeToPNG();
            string base64 = "data:image/png;base64," + System.Convert.ToBase64String(img);
            return base64;
        }

    }

}
