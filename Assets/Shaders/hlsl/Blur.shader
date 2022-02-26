Shader "Hidden/Custom RP/Post FX Stack" {

	SubShader{
		Cull Off
		ZTest Always
		ZWrite Off

		HLSLINCLUDE
		#include "Common.hlsl"
		#include "BlurPass.hlsl"
		ENDHLSL

		Pass {
			Name "Copy"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment CopyPassFragment
			ENDHLSL
		}
	}
}
