using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]

public class MyPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool useDynamicBatching = true, useGPUInstancing = true, useSPRBatcher = true;
    [SerializeField]
    ShadowSettings shadowSettings = default;
    [SerializeField]
    ShadowPostSettings shadowPostSettings = default;
    [SerializeField]
    NPRSetting nprSetting = default;
    [SerializeField]
    AOSetting aoSetting = default;
    [SerializeField]
    bool useDeferredRendering = false;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float ssrStepRatio = 0.1f;
    protected override RenderPipeline CreatePipeline() => new MyPipline(useDynamicBatching,useGPUInstancing,useSPRBatcher,useDeferredRendering,
        shadowSettings, shadowPostSettings, nprSetting, aoSetting, ssrStepRatio);
}


