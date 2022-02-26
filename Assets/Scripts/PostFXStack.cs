using UnityEngine;
using UnityEngine.Rendering;
public class PostFXStack
{

	const string bufferName = "Post FX";

	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	ScriptableRenderContext context;

	Camera camera;

	PostFXSettings settings;
	int fxSourceId = Shader.PropertyToID("_PostFXSource");
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
	public void Render(int sourceId, int targetId)
	{
		Draw(sourceId, targetId, Pass.Copy);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	void Draw(
		RenderTargetIdentifier from, RenderTargetIdentifier to, Pass pass
	)
	{
		buffer.SetGlobalTexture(fxSourceId, from);
		buffer.SetRenderTarget(
			to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
		);
		buffer.ClearRenderTarget(true, false, Color.clear);
		buffer.DrawProcedural(
			Matrix4x4.identity, settings.Material, (int)pass,
			MeshTopology.Triangles, 3
		);
		context.ExecuteCommandBuffer(buffer);
		buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
	}
}
