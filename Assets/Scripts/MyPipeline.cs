using UnityEngine;
using UnityEngine.Rendering;

public class MyPipline : RenderPipeline
{
    CameraRender m_render = new CameraRender();
    bool m_useDynamicBatching, m_useGPUIstancing;
    ShadowSettings m_shadowSettings;
    public MyPipline(bool useDynamicBatching,bool useGPUInstancing, bool useSPRBatcher, ShadowSettings shadowSettings)
    {
        this.m_useDynamicBatching = useDynamicBatching;
        this.m_useGPUIstancing = useGPUInstancing;
        this.m_shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSPRBatcher;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_render.Render(context, camera,m_useDynamicBatching,m_useGPUIstancing,m_shadowSettings);
        }

    }
}
