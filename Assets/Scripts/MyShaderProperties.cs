
using UnityEngine;
public class ScreenProperties
{
    public int uv2ViewID { get; private set; }
    public int zBufferParamID { get; private set; }
    public int texelSizeID { get; private set; }
    public int downScaleTexelSizeID { get; private set; }

    public Vector4 uv2View;
    public Vector4 zBufferParam;
    public Vector4 texelSize;

    public ScreenProperties()
    {
        uv2ViewID = Shader.PropertyToID("_UV2View");
        zBufferParamID = Shader.PropertyToID("_ZBufferParam");
        texelSizeID = Shader.PropertyToID("_TexelSize");
        downScaleTexelSizeID = Shader.PropertyToID("_DownScaleTexelSize");

    }

}

public class TAAProperties
{
    public int preVID { get; private set; }
    public int prePID { get; private set; }
    public int jitterID { get; private set; }

    public TAAProperties()
    {
        preVID = Shader.PropertyToID("_PreV");
        prePID = Shader.PropertyToID("_PreP");
        jitterID = Shader.PropertyToID("_Jitter");
    }
}

public class HBAOProperties
{
    public int radiusID { get; private set; }
    public int radiusPixelID { get; private set; }
    public int maxRadiusPixelID { get; private set; }
    public int angleBiasID { get; private set; }
    public int aoStrengthID { get; private set; }
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

    public int samplesID { get; private set; }
    public int ditherID { get; private set; }
    public int isHorizonID { get; private set; }
    public int ssrStepRatioID { get; private set; }
    public TAAProperties taaPropreties { get; private set; }
    public ScreenProperties screenProperties { get; private set; }
    public HBAOProperties hbaoPropreties { get; private set; }
    public MyShaderProperties()
    {
        samplesID = Shader.PropertyToID("samples");
        ditherID = Shader.PropertyToID("_Dither");
        ssrStepRatioID = Shader.PropertyToID("stepRatio");
        isHorizonID = Shader.PropertyToID("_IsHorizon");
        taaPropreties = new TAAProperties();
        screenProperties = new ScreenProperties();
        hbaoPropreties = new HBAOProperties();
    }
}
