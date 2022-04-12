#ifndef SSAO_PASS_INCLUDE
#define SSAO_PASS_INCLUDE

#ifndef SAMPLE_COUNT
#define SAMPLE_COUNT 64
#endif

float3 CalculateLightVolume(int index, float3 posWS, ShadowData shadowData)
{
	float3 viewDir = _WorldSpaceCameraPos - posWS;
	float step = length(viewDir) / SAMPLE_COUNT;
	viewDir = normalize(viewDir);
	float3 color = 0.0;
	SimpleLight slight;
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		float3 tempPos = posWS + step * viewDir;
		slight = GetSimpleLight(index, tempPos, shadowData);
		color += slight.attenuation * slight.color;
	}
	color /= (SAMPLE_COUNT * 3.0);
	return color;
	
}

#endif //SSAO_PASS_INCLUDE