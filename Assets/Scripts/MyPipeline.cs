using UnityEngine;
using UnityEngine.Rendering;

public class MyPipline : RenderPipeline
{
    CameraRender m_render = new CameraRender();
    bool m_useDynamicBatching, m_useGPUIstancing;
    bool m_useDeferredRendering;
    ShadowSettings m_shadowSettings;
    ShadowPostSettings m_postFXSettings;
    NPRSetting m_nprSettings;
    AOSetting m_aoSettings;
    public MyPipline(bool useDynamicBatching,bool useGPUInstancing, bool useSPRBatcher, bool useDeferredRendering, ShadowSettings shadowSettings, ShadowPostSettings postFXSettings,
        NPRSetting nprSettings, AOSetting aoSetting)
    {
        this.m_useDynamicBatching = useDynamicBatching;
        this.m_useGPUIstancing = useGPUInstancing;
        this.m_shadowSettings = shadowSettings;
        this.m_postFXSettings = postFXSettings;
        this.m_useDeferredRendering = useDeferredRendering;
        this.m_nprSettings = nprSettings;
        this.m_aoSettings = aoSetting;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSPRBatcher;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_render.Render(context, camera,m_useDynamicBatching,m_useGPUIstancing,m_useDeferredRendering,m_shadowSettings, m_postFXSettings,
                m_nprSettings, m_aoSettings);
        }
    }
}
