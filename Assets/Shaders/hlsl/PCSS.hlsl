#ifndef PCSS_INCLUDED
#define PCSS_INCLUDED

uniform float Blocker_Samples = 32;
uniform float PCF_Samples = 32;

uniform float Blocker_Rotation = .5;
uniform float PCF_Rotation = .5;

uniform float Softness = 1.0;
uniform float SoftnessFalloff = 1.0;

uniform float Blocker_GradientBias = 0.0;
uniform float PCF_GradentBias = 1.0;
uniform float CascadeBlendDistance = .5;

uniform float PenumbraWithMaxSamples = .15;

//#if defined(POISSON_32)
static const float2 PoissonOffsets[32] = {
	float2(0.06407013, 0.05409927),
	float2(0.7366577, 0.5789394),
	float2(-0.6270542, -0.5320278),
	float2(-0.4096107, 0.8411095),
	float2(0.6849564, -0.4990818),
	float2(-0.874181, -0.04579735),
	float2(0.9989998, 0.0009880066),
	float2(-0.004920578, -0.9151649),
	float2(0.1805763, 0.9747483),
	float2(-0.2138451, 0.2635818),
	float2(0.109845, 0.3884785),
	float2(0.06876755, -0.3581074),
	float2(0.374073, -0.7661266),
	float2(0.3079132, -0.1216763),
	float2(-0.3794335, -0.8271583),
	float2(-0.203878, -0.07715034),
	float2(0.5912697, 0.1469799),
	float2(-0.88069, 0.3031784),
	float2(0.5040108, 0.8283722),
	float2(-0.5844124, 0.5494877),
	float2(0.6017799, -0.1726654),
	float2(-0.5554981, 0.1559997),
	float2(-0.3016369, -0.3900928),
	float2(-0.5550632, -0.1723762),
	float2(0.925029, 0.2995041),
	float2(-0.2473137, 0.5538505),
	float2(0.9183037, -0.2862392),
	float2(0.2469421, 0.6718712),
	float2(0.3916397, -0.4328209),
	float2(-0.03576927, -0.6220032),
	float2(-0.04661255, 0.7995201),
	float2(0.4402924, 0.3640312),
};

//#else
//static const float2 PoissonOffsets[64] = {
//	float2(0.0617981, 0.07294159),
//	float2(0.6470215, 0.7474022),
//	float2(-0.5987766, -0.7512833),
//	float2(-0.693034, 0.6913887),
//	float2(0.6987045, -0.6843052),
//	float2(-0.9402866, 0.04474335),
//	float2(0.8934509, 0.07369385),
//	float2(0.1592735, -0.9686295),
//	float2(-0.05664673, 0.995282),
//	float2(-0.1203411, -0.1301079),
//	float2(0.1741608, -0.1682285),
//	float2(-0.09369049, 0.3196758),
//	float2(0.185363, 0.3213367),
//	float2(-0.1493771, -0.3147511),
//	float2(0.4452095, 0.2580113),
//	float2(-0.1080467, -0.5329178),
//	float2(0.1604507, 0.5460774),
//	float2(-0.4037193, -0.2611179),
//	float2(0.5947998, -0.2146744),
//	float2(0.3276062, 0.9244621),
//	float2(-0.6518704, -0.2503952),
//	float2(-0.3580975, 0.2806469),
//	float2(0.8587891, 0.4838005),
//	float2(-0.1596546, -0.8791054),
//	float2(-0.3096867, 0.5588146),
//	float2(-0.5128918, 0.1448544),
//	float2(0.8581337, -0.424046),
//	float2(0.1562584, -0.5610626),
//	float2(-0.7647934, 0.2709858),
//	float2(-0.3090832, 0.9020988),
//	float2(0.3935608, 0.4609676),
//	float2(0.3929337, -0.5010948),
//	float2(-0.8682281, -0.1990303),
//	float2(-0.01973724, 0.6478714),
//	float2(-0.3897587, -0.4665619),
//	float2(-0.7416366, -0.4377831),
//	float2(-0.5523247, 0.4272514),
//	float2(-0.5325066, 0.8410385),
//	float2(0.3085465, -0.7842533),
//	float2(0.8400612, -0.200119),
//	float2(0.6632416, 0.3067062),
//	float2(-0.4462856, -0.04265022),
//	float2(0.06892014, 0.812484),
//	float2(0.5149567, -0.7502338),
//	float2(0.6464897, -0.4666451),
//	float2(-0.159861, 0.1038342),
//	float2(0.6455986, 0.04419327),
//	float2(-0.7445076, 0.5035095),
//	float2(0.9430245, 0.3139912),
//	float2(0.0349884, -0.7968109),
//	float2(-0.9517487, 0.2963554),
//	float2(-0.7304786, -0.01006928),
//	float2(-0.5862702, -0.5531025),
//	float2(0.3029106, 0.09497032),
//	float2(0.09025345, -0.3503742),
//	float2(0.4356628, -0.0710125),
//	float2(0.4112572, 0.7500054),
//	float2(0.3401214, -0.3047142),
//	float2(-0.2192158, -0.6911137),
//	float2(-0.4676369, 0.6570358),
//	float2(0.6295372, 0.5629555),
//	float2(0.1253822, 0.9892166),
//	float2(-0.1154335, 0.8248222),
//	float2(-0.4230408, -0.7129914),
//};
//#endif

inline float ValueNoise(float3 pos)
{
	float3 Noise_skew = pos + 0.2127 + pos.x * pos.y * pos.z * 0.3713;
	float3 Noise_rnd = 4.789 * sin(489.123 * (Noise_skew));
	return frac(Noise_rnd.x * Noise_rnd.y * Noise_rnd.z * (1.0 + Noise_skew.x));
}
inline float2 Rotate(float2 pos, float2 rotationTrig)
{
	return float2(pos.x * rotationTrig.x - pos.y * rotationTrig.y,  pos.y * rotationTrig.x + pos.x * rotationTrig.y);
}

float2 FindBlocker(float2 uv, float depth, float scale, float searchUV, float3 unBiasedPositionSTS, float rotationTrig)
{
	float avgBlockerDepth = 0.0;
	float numBlockers = 0.0;
	float blockerSum = 0.0;

	for (int i = 0; i < 32; i++)
	{
		float2 offset = PoissonOffsets[i] * searchUV * scale;

		offset = Rotate(offset, rotationTrig);

        float shadowMapDepth = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas, sampler_DirectionalShadowAtlas, uv + offset).r;

        float biasedDepth = depth;
#if defined(UNITY_REVERSED_Z)
		if(shadowMapDepth > biasedDepth)
#else
		if (shadowMapDepth < biasedDepth)
#endif
		{
			blockerSum += shadowMapDepth;
			numBlockers += 1.0;
		}
	}
	if(numBlockers > 1.0)
		avgBlockerDepth = blockerSum / numBlockers;

#if defined(UNITY_REVERSED_Z)
	avgBlockerDepth = 1.0 - avgBlockerDepth;
#endif

	return float2(avgBlockerDepth, numBlockers);
}

float PCF_Filter(float2 uv, float depth, float scale, float filterRadiusUV, float penumbra, float2 rotationTrig)
{
	float sum = 0.0f;

	for (int i = 0; i < 32; i++)
	{
		float2 offset = PoissonOffsets[i] * filterRadiusUV * scale;

		offset = Rotate(offset, rotationTrig);
		float biasedDepth = depth;

		float value = SamplerDirectionalShadowAtlas(float3(uv.xy + offset, biasedDepth));
		sum += value;
	}

	sum /= 32;
	return sum;
}

float PCSS_Shadow_Calculate(float3 coords, float3 unBiasedPositionSTS, float random, float scale)
{
	float2 uv = coords.xy;
	float depth = coords.z;
	float zAwareDepth = depth;

#if defined(UNITY_REVERSED_Z)
	zAwareDepth = 1.0 - depth;
#endif
	float rotationAngle = random * 3.1415926;
	float rotationTrig = float2(cos(rotationAngle), sin(rotationAngle));

	float searchSize = 0.003 * saturate(zAwareDepth - .02) / zAwareDepth;
    float2 blockerInfo = FindBlocker(uv, depth, scale, searchSize, unBiasedPositionSTS, rotationTrig);

	if (blockerInfo.y < 1)
	{
		return 1.0;
	}

	float penumbra = zAwareDepth - blockerInfo.x;
    float filterRadiusUV = penumbra * 0.05;

	float shadow = PCF_Filter(uv, depth, scale, filterRadiusUV, penumbra, rotationTrig);
	return shadow;
}

#endif //PCSS_INCLUDED