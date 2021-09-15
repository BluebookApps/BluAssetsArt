using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NG
{
    public class SSHUtil
    {
        public static Texture2D CombineCamerasNonThreaded(
            Camera[] cameras, ShotSize shot, TextureFormat format, int depth)
        {
            Texture2D result = null;
            for (int i = 0; i < cameras.Length; i++)
            {
                // Skip cameras who already render to texture.
                if (cameras[i].targetTexture != null)
                    continue;

                RenderTexture rtNew = new RenderTexture(shot.width, shot.height, depth);

                cameras[i].targetTexture = rtNew;
                var tempTex = new Texture2D(shot.width, shot.height, format, false);
                cameras[i].Render();
                RenderTexture.active = rtNew;

                tempTex.ReadPixels(new Rect(0, 0, shot.width, shot.height), 0, 0);
                tempTex.Apply();
                if (i == 0)
                {
                    result = tempTex;
                }
                else
                {
                    result = Combine(result, tempTex);
                }

                cameras[i].targetTexture = null;
                RenderTexture.active = null;
                Object.Destroy(tempTex);
                Object.Destroy(rtNew);
            }

            return result;
        }

        public static Texture2D Combine(Texture2D background, Texture2D foreground)
        {
            for (int x = 0; x < background.width; x++)
            {
                for (int y = 0; y < background.height; y++)
                {
                    var bgColor = background.GetPixel(x, y);
                    var fgColor = foreground.GetPixel(x, y);
                    // skip if fg alpha is less than 1/255
                    if (fgColor.a > 0.003f)
                    {
                        var combined = Color.Lerp(bgColor, fgColor, fgColor.a);
                        background.SetPixel(x, y, combined);
                    }
                }
            }

            background.Apply();
            return background;
        }

        public static void CombineTexturesThread(
            List<Color[]> textureColors, Action<Color[]> onComplete)
        {
            new Thread(() =>
            {
                var bg = textureColors[0];
                for (int i = 1; i < textureColors.Count; i++)
                {
                    var fg = textureColors[i];
                    bg = CombineColorArray(bg, textureColors[i]);
                }

                onComplete.Invoke(bg);
            }).Start();
        }

        public static Color[] CombineColorArray(Color[] background, Color[] foreground)
        {
            int min = Mathf.Min(background.Length, foreground.Length);

            for (int i = 0; i < min; i++)
            {
                // skip if fg alpha is less than 1/255
                if (foreground[i].a > 0.003f)
                {
                    background[i] = Color.Lerp(background[i], foreground[i], foreground[i].a);
                }
            }

            return background;
        }

        public static List<Color[]> GetCameraTexturesColors(
            Camera[] cameras, ShotSize shot, TextureFormat format, int depth)
        {
            var colors = new List<Color[]>();


            for (int i = 0; i < cameras.Length; i++)
            {
                // Skip cameras who already render to texture.
                if (cameras[i].targetTexture != null)
                    continue;

                RenderTexture rtNew = new RenderTexture(shot.width, shot.height, depth);

                cameras[i].targetTexture = rtNew;
                var tempTex = new Texture2D(shot.width, shot.height, format, false);
                cameras[i].Render();
                RenderTexture.active = rtNew;

                tempTex.ReadPixels(new Rect(0, 0, shot.width, shot.height), 0, 0);
                tempTex.Apply();
                colors.Add(tempTex.GetPixels());

                cameras[i].targetTexture = null;
                RenderTexture.active = null;
                Object.Destroy(tempTex);
                Object.Destroy(rtNew);
            }

            return colors;
        }

        public static void SaveTexture(Texture2D tex, string path, string fileName)
        {
            byte[] screenshot = tex.EncodeToPNG(); // can't be threaded :(
            var file = File.Open(Path.Combine(path, fileName), FileMode.Create);
            var binaryWriter = new BinaryWriter(file);
            binaryWriter.Write(screenshot);
            file.Close();
            Object.Destroy(tex);
        }

        public static Texture2D GetScreenBilinearScaling(
            TextureFormat textureFormat, ShotSize shot)
        {
            var tex = new Texture2D(Screen.width, Screen.height, textureFormat, false);
            // Deprecated - was used to handle issue where Unity Editor would capture the 
            // entire Game window resulting in letterboxing or an offset.
            //Vector2 camUpperLeft = mainCamera.pixelRect.min; //lower left
            //camUpperLeft.y = mainCamera.pixelRect.min.y;

            float offsetX = 0f;
            float offsetY = 0f;

            tex.ReadPixels(new Rect(offsetX, offsetY, Screen.width, Screen.height), 0, 0);
            tex.Apply();
            TextureScale.Bilinear(tex, shot.width, shot.height);

            return tex;
        }
    }
}
