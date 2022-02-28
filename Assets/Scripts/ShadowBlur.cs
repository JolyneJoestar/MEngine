using UnityEngine;
using UnityEngine.Rendering;
public class ShadowBlur
{

    const string bufferName = "Shadow Blur";

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    Camera camera;

    PostFXSettings settings;
    int shadowBlurSourceId = Shader.PropertyToID("_ShadowBlurSource");
    int texSize = Shader.PropertyToID("_TexSize");

    enum Pass
    {
        Copy
    }
    public bool IsActive => settings != null;
    public void Setup(
        ScriptableRenderContext context, Camera camera, PostFXSettings settings
    )
    {
        this.context = context;
        this.camera = camera;
        this.settings = settings;
    }
    public void Render(int sourceId, int targetId, int size)
    {
        Draw(sourceId, targetId, Pass.Copy, size);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Draw(
        RenderTargetIdentifier from, RenderTargetIdentifier to, Pass pass, int size
    )
    {
        buffer.SetGlobalTexture(shadowBlurSourceId, from);
        buffer.SetRenderTarget(
            to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );
        buffer.ClearRenderTarget(true, false, Color.clear);
        buffer.SetGlobalInt(texSize, size);
        buffer.DrawProcedural(
            Matrix4x4.identity, settings.Material, (int)pass,
            MeshTopology.Triangles, 3
        );
        buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
    }
}
