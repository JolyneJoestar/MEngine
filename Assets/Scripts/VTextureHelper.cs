using UnityEngine;
using System.IO;
using UnityEditor;

public class VTextureHelper 
{
    private static int s_PageSize = 256;
    private static int s_PaddingSize = 4;
    private static int s_TableSize = 64;

    public static void GenerateVirtualTexture()
    {
        Debug.Log("Generating VirtualTexture...");

        var saveDir = Application.streamingAssetsPath;
        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);

        int maxLevel = (int)Mathf.Log(s_TableSize, 2);
        for (int mip = 0; mip <= maxLevel; mip++)
        {
            if (mip == 0)
            {
                GenerateMip0Cache();
            }
            else
            {
                GenerateCache(mip);
            }

            ExportCache(mip);
        }

        Debug.Log("GenerateVirtualTexture Done.");
        AssetDatabase.Refresh();
    }

    public static void GenerateMip0Cache()
    {
        int rowCount = 12;
        int columnCount = 10;
        int srcTextureWidth = 1024;
        int srcTextureHeight = 768;

        var cache = GetCache(0);
        for(var row = 0; row < rowCount; row++)
        {
            for(var column = 0; column < columnCount; column++)
            {
                var inputFile = Path.Combine("Assets", "Examples", "VirtualTexture", "Slides",
                    string.Format("slide_{0}.JPG", (row * columnCount + column + 1).ToString("D3")));
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(inputFile);
                if (texture == null)
                    continue;
                cache.SetPixel(
                    column * srcTextureWidth,
                    row * srcTextureHeight,
                    texture.width,
                    texture.height,
                    texture.GetRawTextureData());
            }
        }
        Debug.Log("Generate Mip0Cache Done!");
    }

    public static void GenerateCache(int mip)
    {
        var inputCache = GetCache(mip -1);
        var outputCache = GetCache(mip);

        int patchSize = s_PageSize;
        int patchSizeDouble = patchSize * 2;
        int patchCount = outputCache.Size / patchSize;

        var inputTexture = new Texture2D(patchSizeDouble, patchSizeDouble, TextureFormat.RGB24, false);
        var outputTexture = new Texture2D(patchSize, patchSize, TextureFormat.RGB24, false);
        var renderTexture = RenderTexture.GetTemporary(patchSize, patchSize);
        var lastActiveRT = RenderTexture.active;

        for(int row = 0; row < patchCount; row++)
        {
            for(int column = 0; column < patchSize; column++)
            {
                var inputPixels = inputCache.GetPixels(column * patchSizeDouble, row * patchSizeDouble, patchSizeDouble, patchSizeDouble);
                inputTexture.LoadRawTextureData(inputPixels);
                inputTexture.Apply(false);

                Graphics.Blit(inputTexture, renderTexture);

                outputTexture.ReadPixels(new Rect(0, 0, patchSize, patchSize), 0, 0, false);
                outputTexture.Apply(false);

                var outputPixels = outputTexture.GetRawTextureData();
                outputCache.SetPixel(column * patchSize, row * patchSize, patchSize, patchSize, outputPixels);

            }
        }

        RenderTexture.active = lastActiveRT;
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    public static VTextureCache GetCache(int mip)
    {
        var dir = Path.Combine(Application.dataPath, "Examples", "VirtualTexture", "Cache");
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var size = (s_TableSize * s_PageSize) >> mip;
        var cache = new VTextureCache(size, dir, "mip" + mip);
        return cache;
    }

    public static void ExportCache(int mip)
    {
        Debug.LogFormat("Exporting Mip{0}!", mip);
        var cache = GetCache(mip);

        int pageSizeWithPadding = s_PaddingSize + s_PaddingSize * 2;
        int pageCount = cache.Size / s_PageSize;

        for(int row = 0; row < pageCount; row++)
        {
            for(int column = 0; column < pageCount; column++)
            {
                var pixels = cache.GetPixels(
                    column * s_PageSize - s_PaddingSize,
                    row * s_PageSize - s_PaddingSize,
                    pageSizeWithPadding,
                    pageSizeWithPadding);
                var texture = new Texture2D(pageSizeWithPadding, pageSizeWithPadding, TextureFormat.RGB24, false);
                texture.LoadRawTextureData(pixels);
                texture.Apply(false);

                var bytes = ImageConversion.EncodeToPNG(texture);
                var file = Path.Combine(Application.streamingAssetsPath, string.Format("Slide_MIP{0}_Y{1}_X{2}", mip, row, column));
                File.WriteAllBytes(file, bytes);
            }
        }
        Debug.LogFormat("Expot Mip{0} Done!", mip);
    }
}
