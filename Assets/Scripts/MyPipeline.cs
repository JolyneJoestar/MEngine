using UnityEngine;
using UnityEngine.Rendering;

public class MyPipline : RenderPipeline
{
    CameraRender m_render = new CameraRender();
    bool m_useDynamicBatching, m_useGPUIstancing;
    ShadowSettings m_shadowSettings;
    ShadowPostSettings m_postFXSettings;
    public MyPipline(bool useDynamicBatching,bool useGPUInstancing, bool useSPRBatcher, ShadowSettings shadowSettings, ShadowPostSettings postFXSettings)
    {
        this.m_useDynamicBatching = useDynamicBatching;
        this.m_useGPUIstancing = useGPUInstancing;
        this.m_shadowSettings = shadowSettings;
        this.m_postFXSettings = postFXSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSPRBatcher;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_render.Render(context, camera,m_useDynamicBatching,m_useGPUIstancing,m_shadowSettings, m_postFXSettings);
        }

    }
}
