using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]

public class MyPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool useDynamicBatching = true, useGPUInstancing = true, useSPRBatcher = true;
    [SerializeField]
    ShadowSettings shadowSettings = default;
    protected override RenderPipeline CreatePipeline() => new MyPipline(useDynamicBatching,useGPUInstancing,useSPRBatcher,shadowSettings);
}


