using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

public interface ILoader
{
    event Action<LoadRequest, Texture2D[]> OnLoadComplete;

    LoadRequest Request(int x, int y, int mip);
}

public class FileLoader : MonoBehaviour, ILoader
{
    public event Action<LoadRequest, Texture2D[]> OnLoadComplete;

    private int m_ThreadLimit = 1;
    private FolderType m_fileRoot;
    private string[] m_filePathStrs;
    private List<LoadRequest> m_runingRequests = new List<LoadRequest>();
    private List<LoadRequest> m_pendingRequests = new List<LoadRequest>();

    private IEnumerator Load(LoadRequest request)
    {
        Texture2D[] textures = new Texture2D[m_filePathStrs.Length];
        for (int i = 0; i < m_filePathStrs.Length; i++)
        {
            var file = string.Format(m_filePathStrs[i], request.PageX >> request.MipLevel, request.PageY >> request.MipLevel, request.MipLevel);
            var www = UnityWebRequestTexture.GetTexture(file);
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                textures[i] = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }
            else
            {
                Debug.LogWarningFormat("Load file{0} failed: {1}", file, www.error);
            }
        }
        m_runingRequests.Remove(request);
        OnLoadComplete?.Invoke(request, textures);
    }

    public LoadRequest Request(int x, int y, int mip)
    {
        foreach(var r in m_runingRequests)
        {
            if (r.PageX == x && r.PageY == y && r.MipLevel == mip)
                return null;
        }
        foreach(var r in m_pendingRequests)
        {
            if (r.PageX == x && r.PageY == y && r.MipLevel == mip)
                return null;
        }

        var request = new LoadRequest(x, y, mip);
        m_pendingRequests.Add(request);
        return request;
    }
}
