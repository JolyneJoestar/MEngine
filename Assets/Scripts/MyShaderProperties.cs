
using UnityEngine;
public class ScreenProperties
{
    public int uv2ViewID;
    public int zBufferParamID;
    public int texelSizeID;

    public Vector4 uv2View;
    public Vector4 zBufferParam;
    public Vector4 texelSize;

    public ScreenProperties()
    {
        uv2ViewID = Shader.PropertyToID("_UV2View");
        zBufferParamID = Shader.PropertyToID("_ZBufferParam");
        texelSizeID = Shader.PropertyToID("_TexelSize");
    }

}

public class TAAProperties
{
    public int preVID;
    public int prePID;
    public int jitterID;

    public TAAProperties()
    {
        preVID = Shader.PropertyToID("_PreV");
        prePID = Shader.PropertyToID("_PreP");
        jitterID = Shader.PropertyToID("_Jitter");
    }
}

public class HBAOProperties
{
    public int radiusID;
    public int radiusPixelID;
    public int maxRadiusPixelID;
    public int angleBiasID;
    public int aoStrengthID;
    public HBAOProperties()
    {
        radiusID = Shader.PropertyToID("_Radius");
        radiusPixelID = Shader.PropertyToID("_RadiusPixel");
        maxRadiusPixelID = Shader.PropertyToID("_MaxRadiusPixel");
        angleBiasID = Shader.PropertyToID("_AngleBias");
        aoStrengthID  = Shader.PropertyToID("_AOStrength");
    }
}


public class MyShaderProperties
{

    public int samplesID;
    public int ditherID;
    public int bloomInputID;
    public int ssrStepRatioID;
    public TAAProperties taaPropreties;
    public ScreenProperties screenProperties;
    public HBAOProperties hbaoPropreties;
    public MyShaderProperties()
    {
        samplesID = Shader.PropertyToID("samples");
        ditherID = Shader.PropertyToID("_Dither");
        ssrStepRatioID = Shader.PropertyToID("stepRatio");
        taaPropreties = new TAAProperties();
        screenProperties = new ScreenProperties();
        hbaoPropreties = new HBAOProperties();
    }
}
