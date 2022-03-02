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
    float z = GetSource(input.screenUV);
    float sinFactor1 = sin(PI * (2 * 1 - 1) * z);
    float cosFactor1 = cos(PI * (2 * 1 - 1) * z);
    float sinFactor2 = sin(PI * (2 * 2 - 1) * z);
    float cosFactor2 = cos(PI * (2 * 2 - 1) * z);
    output.FourierOutputOne = float4(sinFactor1, cosFactor1, sinFactor2, cosFactor2);
    sinFactor1 = sin(PI * (2 * 3 - 1) * z);
    cosFactor1 = cos(PI * (2 * 3 - 1) * z);
    sinFactor2 = sin(PI * (2 * 4 - 1) * z);
    cosFactor2 = cos(PI * (2 * 4 - 1) * z);
    output.FourierOutputTwo = float4(sinFactor1, cosFactor1, sinFactor2, cosFactor2);
    sinFactor1 = sin(PI * (2 * 5 - 1) * z);
    cosFactor1 = cos(PI * (2 * 5 - 1) * z);
    sinFactor2 = sin(PI * (2 * 6 - 1) * z);
    cosFactor2 = cos(PI * (2 * 6 - 1) * z);
    output.FourierOutputThree = float4(sinFactor1, cosFactor1, sinFactor2, cosFactor2);
    sinFactor1 = sin(PI * (2 * 7 - 1) * z);
    cosFactor1 = cos(PI * (2 * 7 - 1) * z);
    sinFactor2 = sin(PI * (2 * 8 - 1) * z);
    cosFactor2 = cos(PI * (2 * 8 - 1) * z);
    output.FourierOutputFour = float4(sinFactor1, cosFactor1, sinFactor2, cosFactor2);
    return output;
}
#endif //CONVOLUTION_PRE_PASS_INCLUDED