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




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
