using UnityEngine;
using UnityEngine.Rendering;

public class ConvolutionShadowMap
{
    const string bufferName = "CSM";

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    Camera camera;

    ShadowPostSettings settings;
    int shadowBlurSourceId = Shader.PropertyToID("_ShadowBlurSource");
    int texSize = Shader.PropertyToID("_TexSize");

    public bool IsActive => settings != null;
    public void Setup(
        ScriptableRenderContext context, Camera camera, ShadowPostSettings settings
    )
    {
        this.context = context;
        this.camera = camera;
        this.settings = settings;
    }
    public void Render(int sourceId, int[] targetId, int size)
    {
        RenderTargetIdentifier[] targets = new RenderTargetIdentifier[targetId.Length];
        for(int i = 0; i < targetId.Length; i++)
        {
            targets[i] = targetId[i];
        }
        Draw(sourceId, targets, Pass.Fourier, size);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Draw(
        RenderTargetIdentifier from, RenderTargetIdentifier[] to, Pass pass, int size
    )
    {
        buffer.SetGlobalTexture(shadowBlurSourceId, from);
        buffer.SetRenderTarget(to, BuiltinRenderTextureType.Depth);
        buffer.ClearRenderTarget(true, false, Color.clear);
        buffer.SetGlobalInt(texSize, size);
        buffer.DrawProcedural(
            Matrix4x4.identity, settings.Material, (int)pass,
            MeshTopology.Triangles, 3
        );
        buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
    }
}
