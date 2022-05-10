Shader "MyPipeline/PostProccess"
{
    SubShader
    {
		Pass
		{
			ZWrite Off
			ZTest Off
			Tags { "LightMode" = "BloomInput" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BloomGetSource
			#include "BloomInput.hlsl"

			ENDHLSL
		}

		Pass
		{
			ZWrite Off
			ZTest Off
			Tags { "LightMode" = "BloomBlur" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BloomBlurPass
			#include "BloomBlur.hlsl"

			ENDHLSL
		}

		Pass
		{
			ZWrite Off
			ZTest Off
			Tags { "LightMode" = "Bloom" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BloomFinal
			#include "Bloom.hlsl"
			ENDHLSL
		}
    }
}
