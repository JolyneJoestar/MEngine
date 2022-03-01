Shader "Hidden/Custom RP/Shadow Post Process" {

	SubShader{
		Cull Off
		ZTest Always
		ZWrite Off

		HLSLINCLUDE
		#include "Common.hlsl"
		#include "VSMBlurPass.hlsl"
		ENDHLSL

		Pass {
			Name "VSM"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment CopyPassFragment
			ENDHLSL
		}
	}
}
