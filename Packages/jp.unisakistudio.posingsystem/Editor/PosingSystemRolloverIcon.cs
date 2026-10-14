using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemRolloverIcon
    {
        private const string Path = "Packages/jp.unisakistudio.posingsystem/ExpressionsMenuIcons/PosingSystem_Rollover.png";
        private static Texture2D badge;
        private static Texture2D Badge
        {
            get
            {
                if (badge == null && File.Exists(Path))
                {
                    badge = new Texture2D(2, 2, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
                    badge.LoadImage(File.ReadAllBytes(Path));
                }
                return badge;
            }
        }
        public static void Draw(Rect image)
        {
            if (Badge == null) return;
            var size = image.width * .36f;
            var rect = new Rect(image.x, image.yMax - size, size, size);
            EditorGUI.DrawRect(rect, new Color(.08f, .15f, .23f, 1));
            GUI.DrawTexture(rect, Badge, ScaleMode.ScaleToFit);
        }
        public static void Stamp(Texture2D image)
        {
            if (Badge != null) Stamp(image, Badge);
        }
        public static void Stamp(Texture2D image, Texture2D mark)
        {
            var size = Mathf.Max(1, Mathf.RoundToInt(Mathf.Min(image.width, image.height) * .36f));
            var pixels = image.GetPixels32();
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var overlay = mark.GetPixelBilinear((x + .5f) / size, (y + .5f) / size);
                var background = new Color(.08f, .15f, .23f, 1);
                var color = Color.Lerp(background, overlay, overlay.a); color.a = 1;
                pixels[y * image.width + x] = color;
            }
            image.SetPixels32(pixels); image.Apply();
        }
        public static Texture2D CopyWithBadge(Texture2D source)
        {
            if (source == null) return null;
            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false) { name = source.name + "_Rollover" };
            var path = AssetDatabase.GetAssetPath(source);
            if (source.isReadable) { result.SetPixels(source.GetPixels()); result.Apply(); }
            else if (!string.IsNullOrEmpty(path) && (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg")))
                result.LoadImage(File.ReadAllBytes(path));
            else
            {
                var old = RenderTexture.active;
                var temp = RenderTexture.GetTemporary(source.width, source.height, 0);
                try { Graphics.Blit(source, temp); RenderTexture.active = temp; result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0); result.Apply(); }
                finally { RenderTexture.active = old; RenderTexture.ReleaseTemporary(temp); }
            }
            Stamp(result); return result;
        }
    }
}
