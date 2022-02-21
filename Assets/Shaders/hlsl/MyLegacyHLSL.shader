// Upgrade NOTE: replaced 'glstate_matrix_projection' with 'UNITY_MATRIX_P'

// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyPipeline/LegacyHLSL"
{
	Properties
	{
		MTint("Tint",Color) = (1,1,1,1)
		MTexture("Texture",2D) = "white"{}
		MMetallic("Metallic",Range(0,1)) = 0.5
		MSmoothness("Smoothness",Range(0,1)) = 0.5
		MCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] MClipping("Alpha Clipping", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]MSrcBlend("Src Blend",float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]MDstBlend("Dst Blend", float) = 0
		[Enum(Off,0,On,1)]MZWrite("ZWrite",float) = 1

	}
		SubShader
		{

			Pass
			{
				Blend[MSrcBlend][MDstBlend]
				ZWrite[MZWrite]

				Tags { "LightMode" = "SPRDefaultLegay" }

				HLSLPROGRAM

				#pragma multi_compile_instancing
				#pragma vertex LegacyVertex
				#pragma fragment LegacyFragment

				#include "MyLegacyLightPass.hlsl"

				ENDHLSL
			}

			Pass{
				Tags{
					"LightMode" = "ShadowCaster"
				}

				ColorMask 0

				HLSLPROGRAM
				#pragma target 3.5
				#pragma shader_feature _CLIPPING
				#pragma multi_compile_instancing
				#pragma vertex ShadowCasterPassVertex
				#pragma fragment ShadowCasterPassFragment

				#include "ShadowCasterPass.hlsl"
					
				ENDHLSL
			}
		}
}
