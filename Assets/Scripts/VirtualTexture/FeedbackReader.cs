using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

public enum ScaleFactor
{
    // 埻宎喜渡
    One,

    // 1/2喜渡
    Half,

    // 1/4喜渡
    Quarter,

    // 1/8喜渡
    Eighth,
}

public static class ScaleModeExtensions
{
    public static float ToFloat(this ScaleFactor mode)
    {
        switch (mode)
        {
            case ScaleFactor.Eighth:
                return 0.125f;
            case ScaleFactor.Quarter:
                return 0.25f;
            case ScaleFactor.Half:
                return 0.5f;
        }
        return 1;
    }
}

public interface IFeedbackReader
{
    event Action<Texture2D> OnFeedbackReaderComplete;
}

public class FeedbackReader : MonoBehaviour, IFeedbackReader
{
    public event Action<Texture2D> OnFeedbackReaderComplete;

    private ScaleFactor m_readbackScale;
    private Shader m_downScaleShader;
    private Material m_downScaleMaterial;
    private int m_downScaleMaterialPass;
    private RenderTexture m_downScaleTexture;
    private Queue<AsyncGPUReadbackRequest> m_readbackRequests = new Queue<AsyncGPUReadbackRequest>();
    private Texture2D m_readbackTexture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void NewRequest(RenderTexture texture)
    {
        if (m_readbackRequests.Count > 8)
            return;
        var width = (int)(texture.width * m_readbackScale.ToFloat());
        var height = (int)(texture.height * m_readbackScale.ToFloat());
        
        if(m_readbackScale != ScaleFactor.One)
        {
            if(m_downScaleTexture == null || m_downScaleTexture.width != width || m_downScaleTexture.height != height)
            {
                m_downScaleTexture = new RenderTexture(width, height, 0);
            }
            m_downScaleTexture.DiscardContents();
            Graphics.Blit(texture, m_downScaleTexture, m_downScaleMaterial, m_downScaleMaterialPass);
            texture = m_downScaleTexture;
        }

        if(m_readbackTexture == null || m_readbackTexture.width != width || m_readbackTexture.height != height)
        {
            m_readbackTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            m_readbackTexture.filterMode = FilterMode.Point;
            m_readbackTexture.wrapMode = TextureWrapMode.Clamp;

          
        }

        var request = AsyncGPUReadback.Request(texture);
        m_readbackRequests.Enqueue(request);
    }

    private void UpdateRequest()
    {
        bool complete = false;
        while(m_readbackRequests.Count > 0)
        {
            var req = m_readbackRequests.Peek();
            if(req.hasError)
            {
                m_readbackRequests.Dequeue();
            }
            else if(req.done)
            {
                m_readbackTexture.GetRawTextureData<Color32>().CopyFrom(req.GetData<Color32>());
                complete = true;
                m_readbackRequests.Dequeue();
            }
            else
            {
                break;
            }
        }
        if(complete)
        {
            OnFeedbackReaderComplete?.Invoke(m_readbackTexture);
        }
    }
}
