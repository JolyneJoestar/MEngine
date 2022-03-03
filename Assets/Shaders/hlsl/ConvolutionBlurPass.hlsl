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
#define sigma 1.0

TEXTURE2D(_FourierBlurSourceOne);
SAMPLER(sampler_FourierBlurSourceOne);
TEXTURE2D(_FourierBlurSourceTwo);
SAMPLER(sampler_FourierBlurSourceTwo);
TEXTURE2D(_FourierBlurSourceThree);
SAMPLER(sampler_FourierBlurSourceThree);
TEXTURE2D(_FourierBlurSourceFour);
SAMPLER(sampler_FourierBlurSourceFour);

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

void GetGaussianTent7x7(out float2 offsets[49],out float weight[49])
{
    int length = 7;
    float totalWeight = 0.0;
    for (int i = -3; i <= 3; i++)
    {
        for (int j = -3; j <= 3; j++)
        {
            int index = (i + 3) * length + j + 3;
            offsets[index] = float2(i, j);
            weight[index] = exp(-(i * i + j * j) / (2 * sigma * sigma)) / (2 * PI * sigma * sigma);
            totalWeight += weight[index];
        }

    }
    for (int i = -3; i <= 3; i++)
    {
        for (int j = -3; j <= 3; j++)
        {
            int index = (i + 3) * length + j + 3;
            weight[index] /= totalWeight;
        }
    }
}

FourierOutput FourierBlurPassFragment(Varyings input)
{
    FourierOutput output;
  //  float2 offsets[49] =
  //  {
  //      float2(-1.0, 1.0),
  //      float2(0.0, 1.0),
  //      float2(1.0, 1.0),
  //      float2(-1.0, 0.0),
  //      float2(0.0, 0.0),
  //      float2(1.0, 0.0),
  //      float2(-1.0, -1.0),
  //      float2(0.0, -1.0),
  //      float2(1.0, -1.0)
  //  };
  //  float weights[] =
  //  {
  //      0.0947416, 0.118318, 0.0947416, 0.118318, 0.147761,
		//0.118318, 0.0947416, 0.118318, 0.0947416
  //  };
    float2 offsets[49];
    float weights[49];
    GetGaussianTent7x7(offsets, weights);
    float4 color1 = float4(0.0,0.0,0.0,0.0);
    float4 color2 = float4(0.0, 0.0, 0.0, 0.0);
    float4 color3 = float4(0.0, 0.0, 0.0, 0.0);
    float4 color4 = float4(0.0, 0.0, 0.0, 0.0);
    for (int i = 0; i < 49; i++)
    {
        float2 offset = offsets[i] / 1024;
        color1 += SAMPLE_TEXTURE2D(_FourierBlurSourceOne, sampler_FourierBlurSourceOne, input.screenUV + offset).rgba * weights[i];
        color2 += SAMPLE_TEXTURE2D(_FourierBlurSourceTwo, sampler_FourierBlurSourceTwo, input.screenUV + offset).rgba * weights[i];
        color3 += SAMPLE_TEXTURE2D(_FourierBlurSourceThree, sampler_FourierBlurSourceThree, input.screenUV + offset).rgba * weights[i];
        color4 += SAMPLE_TEXTURE2D(_FourierBlurSourceFour, sampler_FourierBlurSourceFour, input.screenUV + offset).rgba * weights[i];
    }
    output.FourierOutputOne = color1;
    output.FourierOutputTwo = color2;
    output.FourierOutputThree = color3;
    output.FourierOutputFour = color4;
    return output;
}
#endif //CONVOLUTION_PRE_PASS_INCLUDED