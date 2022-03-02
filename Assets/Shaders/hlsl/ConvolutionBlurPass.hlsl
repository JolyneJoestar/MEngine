#ifndef CONVOLUTION_PRE_PASS_INCLUDED
#define CONVOLUTION_PRE_PASS_INCLUDED

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 screenUV : VAR_SCREEN_UV;
};

struct FourierOutput
{
    float4 FourierOutputOne : SV_Target0;
    float4 FourierOutputTwo : SV_Target1;
    float4 FourierOutputThree : SV_Target2;
    float4 FourierOutputFour : SV_Target3;
};
int _TexSize;


TEXTURE2D(_FourierBlurSourceOne);
SAMPLER(sampler_FourierBlurSourceOne);
TEXTURE2D(_FourierBlurSourceTwo);
SAMPLER(sampler_FourierBlurSourceTwo);
TEXTURE2D(_FourierBlurSourceThree);
SAMPLER(sampler_FourierBlurSourceThree);
TEXTURE2D(_FourierBlurSourceThree);
SAMPLER(sampler_FourierBlurSourceThree);

Varyings DefaultPassVertex(uint vertexID : SV_VertexID)
{
    Varyings output;
    output.positionCS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? -3.0 : 1.0,
		0.0, 1.0
	);
    output.screenUV = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
    return output;
}

FourierOutput FourierGenPassFragment(Varyings input)
{
    FourierOutput output;
    float offsets[] =
    {
        -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0
    };
    float weights[] =
    {
        0.01621622, 0.05405405, 0.12162162, 0.19459459, 0.22702703,
		0.19459459, 0.12162162, 0.05405405, 0.01621622
    };
    float4 color1 = float4(0.0,0.0,0.0,0.0);
    float4 color2 = float4(0.0, 0.0, 0.0, 0.0);
    float4 color3 = float4(0.0, 0.0, 0.0, 0.0);
    float4 color4 = float4(0.0, 0.0, 0.0, 0.0);
    for (int i = 0; i < 9; i++)
    {
        float offset = offsets[i] * GetSourceTexelSize().y;
        color1 += SAMPLE_TEXTURE2D(_FourierBlurSourceOne, sampler_FourierBlurSourceOne, screenUV).rgba * weights[i];
        color2 += SAMPLE_TEXTURE2D(_FourierBlurSourceTwo, sampler_FourierBlurSourceTwo, screenUV).rgba * weights[i];
        color3 += SAMPLE_TEXTURE2D(_FourierBlurSourceThree, sampler_FourierBlurSourceThree, screenUV).rgba * weights[i];
        color4 += SAMPLE_TEXTURE2D(_FourierBlurSourceFour, sampler_FourierBlurSourceFour, screenUV).rgba * weights[i];
    }
    output.FourierOutputOne = color1;
    output.FourierOutputTwo = color2;
    output.FourierOutputThree = color3;
    output.FourierOutputFour = color4;
    return output;
}
#endif //CONVOLUTION_PRE_PASS_INCLUDED