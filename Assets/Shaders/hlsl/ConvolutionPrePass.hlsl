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


TEXTURE2D(_ShadowBlurSource);
SAMPLER(sampler_ShadowBlurSource);

float GetSource(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_ShadowBlurSource, sampler_ShadowBlurSource, screenUV).r;
}

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
    output.FourierOutputOne = float4(GetSource(input.screenUV), 0, 0, 1);
    output.FourierOutputTwo = float4(0.3, GetSource(input.screenUV), 0.3, 1);
    output.FourierOutputThree = float4(0.5, 0.5, GetSource(input.screenUV), 1);
    output.FourierOutputFour = float4(0.8, 0.8, GetSource(input.screenUV), 1);
    return output;
}
#endif //CONVOLUTION_PRE_PASS_INCLUDED