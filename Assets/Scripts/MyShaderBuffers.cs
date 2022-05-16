using UnityEngine;

public class GBuffers
{
    public int GPositionID { get; private set; }
    public int GNormalID { get; private set; }
    public int GAlbedoID { get; private set; }
    public int GMaterialID { get; private set; }
    public int MaxGBUfferNum { get; private set; }

    public GBuffers()
    {
        GPositionID = Shader.PropertyToID("_GPosition");
        GNormalID = Shader.PropertyToID("_GNormal");
        GAlbedoID = Shader.PropertyToID("_GAlbedo");
        GMaterialID = Shader.PropertyToID("_GMaterial");
        MaxGBUfferNum = 4;
    }
    public int this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return GPositionID;
                case 1: return GNormalID;
                case 2: return GAlbedoID;
                case 3: return GMaterialID;
                default: return -1;
            }
        }
    }

}

public class MyShaderBuffers
{
    public int bluredAoTextureID { get; private set; }
    public int aoTextureID { get; private set; }
    public int lightVolumeTextureID { get; private set; }
    public int bluredLightVolumeTextureID { get; private set; }
    public int baseColorTextureID { get; private set; }
    public int preColorTextureID { get; private set; }
    public int currentColorTextureID { get; private set; }
    public int dfColorTextureID { get; private set; }
    public int noiseTextureID { get; private set; }
    public int bloomInputTextureID { get; private set; }
    public int depthBufferID { get; private set; }

    public GBuffers gbuffers;

    public MyShaderBuffers()
    {
        bluredAoTextureID = Shader.PropertyToID("_BluredAoTexture");
        aoTextureID = Shader.PropertyToID("_AoTexture");
        lightVolumeTextureID = Shader.PropertyToID("_LightVolume");
        bluredLightVolumeTextureID = Shader.PropertyToID("_BluredLightVolume");
        baseColorTextureID = Shader.PropertyToID("_BaseColorBuffer");
        preColorTextureID = Shader.PropertyToID("_PreColorBuffer");
        currentColorTextureID = Shader.PropertyToID("_CurrentColorBuffer");
        dfColorTextureID = Shader.PropertyToID("_DFColorBuffer");
        noiseTextureID = Shader.PropertyToID("_Noise");
        bloomInputTextureID = Shader.PropertyToID("_BloomInput");
        depthBufferID = Shader.PropertyToID("_CameraDepthTex");

        gbuffers = new GBuffers();
    }
}
