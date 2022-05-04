
using UnityEngine;

struct BasicProperties
{
    
}

struct DFLightingPassProperties
{

}

struct LightVolumePassProperties
{

}

struct GeometricPassProperties
{

}


public class MyShaderProperties
{
    public int bluredAoTextureId;
    public int aoTextureId;
    public int samplesId;
    public int noiseId;
    public int lightVolumeId;
    public int bluredLightVolumeId;
    public int ditherId;
    public int DFColorBufferId;
    public int bloomInput;
    public int baseColorBuffer;
    public int preColorBuffer;
    public int currentColorBuffer;
    public int preV;
    public int preP;
    public int jitterId;
    public int depthBufferId;

    public MyShaderProperties()
    {
        bluredAoTextureId = Shader.PropertyToID("_BluredAoTexture");
        aoTextureId = Shader.PropertyToID("_AoTexture");
        samplesId = Shader.PropertyToID("samples");
        noiseId = Shader.PropertyToID("_Noise");
        lightVolumeId = Shader.PropertyToID("_LightVolume");
        bluredLightVolumeId = Shader.PropertyToID("_BluredLightVolume");
        ditherId = Shader.PropertyToID("_Dither");
        DFColorBufferId = Shader.PropertyToID("_DFColorBuffer");
        bloomInput = Shader.PropertyToID("_BloomInput");
        baseColorBuffer = Shader.PropertyToID("_BaseColorBuffer");
        preColorBuffer = Shader.PropertyToID("_PreColorBuffer");
        currentColorBuffer = Shader.PropertyToID("_CurrentColorBuffer");
        preV = Shader.PropertyToID("_PreV");
        preP = Shader.PropertyToID("_PreP");
        jitterId = Shader.PropertyToID("_Jitter");
        depthBufferId = Shader.PropertyToID("_CameraDepthTex");
    }
}
