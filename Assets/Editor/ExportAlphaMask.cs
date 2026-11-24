using UnityEngine;
using UnityEditor;
using System.IO;

public static class ExportAlphaMask
{
    [MenuItem("Assets/Export Alpha As Grayscale")]
    static void Export()
    {
        foreach (var obj in Selection.objects)
        {
            var tex = obj as Texture2D;
            if (tex == null) continue;

            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(tex, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, true);
            copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            var px = copy.GetPixels32();
            for (int i = 0; i < px.Length; i++)
            {
                byte a = px[i].a;
                px[i] = new Color32(a, a, a, 255);
            }
            copy.SetPixels32(px);
            copy.Apply();

            var path = Path.ChangeExtension(AssetDatabase.GetAssetPath(tex), null) + "_alpha.png";
            File.WriteAllBytes(path, copy.EncodeToPNG());
            Object.DestroyImmediate(copy);
        }
        AssetDatabase.Refresh();
    }
}