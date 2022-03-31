using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IFeedBackRender
{
    event Action<RenderTexture> OnFeedbackRenderComplete;
}
public class FeedBackRender : MonoBehaviour, IFeedBackRender
{
    public event Action<RenderTexture> OnFeedbackRenderComplete;

    private ScaleFactor m_scale;
    private Shader m_feedbackShader;
    private int m_mipmapBias;
    private Camera m_feedbackCamera;


    public RenderTexture TargetTexture { get; private set; }
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPreCull()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        var scale = m_scale.ToFloat();
        var width = (int)(mainCamera.pixelWidth * scale);
        var height = (int)(mainCamera.pixelHeight * scale);
        if(TargetTexture == null || TargetTexture.width != width || TargetTexture.height != height)
        {
            TargetTexture = new RenderTexture(width, height, 0);
            TargetTexture.useMipMap = false;
            TargetTexture.wrapMode = TextureWrapMode.Clamp;
            TargetTexture.filterMode = FilterMode.Point;

            m_feedbackCamera.targetTexture = TargetTexture;

            var tileTexture = GetComponent(typeof(ITiledTexture)) as ITiledTexture;
            var virtualTable = GetComponent()
        }
    }
}
