using UnityEngine;
using System;
using System.Collections;
using System.IO;

public interface ILoader
{
    event Action<LoadRequest, Texture2D[]> OnLoadComplete;

    LoadRequest Request(int x, int y, int mip);
}

public class FileLoader : MonoBehaviour, ILoader
{
    public event Action<LoadRequest, Texture2D[]> OnLoadComplete;

    private int m_ThreadLimit = 1;

}
