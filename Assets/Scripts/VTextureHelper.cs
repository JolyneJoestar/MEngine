using UnityEngine;
using System.IO;

public class VTextureHelper 
{
    public static void GenerateMip0Cache()
    {
        int rowCount = 12;
        int columnCount = 10;
        int srcTextureWidth = 1024;
        int srcTextureHeight = 768;

        var cache = GetCache(0);
    }

    public static VTextureCache GetCache(int mip)
    {
        var dir = Path.Combine(Application.dataPath)
        return 
    }
}
