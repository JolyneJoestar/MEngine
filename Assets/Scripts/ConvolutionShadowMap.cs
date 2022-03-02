using UnityEngine;
using UnityEngine.Rendering;

public class ConvolutionShadowMap
{
    const string bufferName = "CSM";

    const int MAX_FOURIEPRETEXTURE = 4;
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    Camera camera;

    ShadowPostSettings settings;
    int shadowBlurSourceId = Shader.PropertyToID("_ShadowBlurSource");
    int texSize = Shader.PropertyToID("_TexSize");

    static int[] fourierBlurSourceDataId = 
    {
        Shader.PropertyToID("_FourierBlurSourceOne"),
        Shader.PropertyToID("_FourierBlurSourceTwo"),
        Shader.PropertyToID("_FourierBlurSourceThree"),
        Shader.PropertyToID("_FourierBlurSourceFour")
    };
    RenderTargetIdentifier[] targets = new RenderTargetIdentifier[MAX_FOURIEPRETEXTURE];
    RenderTargetIdentifier[] blurSource = new RenderTargetIdentifier[MAX_FOURIEPRETEXTURE];
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
        for(int i = 0; i < fourierBlurSourceDataId.Length; i++)
        {
            if(i == 0)
                buffer.GetTemporaryRT(fourierBlurSourceDataId[i], size, size, 32, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            else
                buffer.GetTemporaryRT(fourierBlurSourceDataId[i], size, size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        }
        ExecuteBuffer();
        for (int i = 0; i < targetId.Length; i++)
        {
            blurSource[i] = new RenderTargetIdentifier(fourierBlurSourceDataId[i]);
        }
        Draw(sourceId, Pass.Fourier, size);
        ExecuteBuffer();
        for (int i = 0; i < targetId.Length; i++)
        {
            targets[i] = new RenderTargetIdentifier(targetId[i]);
        }
        DrawBlur(targets,size);
        ExecuteBuffer();
    }

    void Draw(
        RenderTargetIdentifier from, Pass pass, int size
    )
    {
        buffer.SetGlobalTexture(shadowBlurSourceId, from);
        buffer.SetRenderTarget(blurSource, blurSource[0]);
        buffer.ClearRenderTarget(true, true, Color.white);
        buffer.SetGlobalInt(texSize, size);
        buffer.DrawProcedural(
            Matrix4x4.identity, settings.Material, (int)pass,
            MeshTopology.Triangles, 3
        );
        buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
    }

    void DrawBlur(RenderTargetIdentifier[] to, int size)
    {
        for(int i = 0; i < fourierBlurSourceDataId.Length; i++)
        {
            buffer.SetGlobalTexture(fourierBlurSourceDataId[i], blurSource[i]);
        }
        buffer.SetRenderTarget(to, to[0]);
        buffer.ClearRenderTarget(true, true, Color.white);
        buffer.SetGlobalInt(texSize, size);
        buffer.DrawProcedural(
            Matrix4x4.identity, settings.Material, (int)2,
            MeshTopology.Triangles, 3
        );
    }
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
