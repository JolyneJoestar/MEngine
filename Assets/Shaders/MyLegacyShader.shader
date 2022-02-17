// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyPipeline/Legacy"
{
	Properties
	{
		MTint("Tint",Color) = (1,1,1,1)
		MTexture("Texture",2D) = "white"{}
		MMetallic("Metallic",Range(0,1)) = 0.5
		MSmoothness("Smoothness",Range(0,1)) = 0.5
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

			CGPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex LegacyVertex
			#pragma fragment LegacyFragment

			#include "UnityCG.cginc"
			#include "MyLegacySurface.cginc"
			#include "MyLegacyLight.cginc"
			#include "MyLegacyBRDF.cginc"


			struct MVertexIn {
			float4 position : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct MVertexOut {
				float4 positionCS : SV_POSITION;
				float3 positionWS : VAR_POSITION;
				float3 normal : VAR_NORMAL;
				float2 uv : VAR_BASE_UV;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D MTexture;

			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
				UNITY_DEFINE_INSTANCED_PROP(float4, MTexture_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, MTint)
				UNITY_DEFINE_INSTANCED_PROP(float, MMetallic)
				UNITY_DEFINE_INSTANCED_PROP(float, MSmoothness)
				UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			// float3 _WorldSpaceCameraPos;

			float3 CalculateDiffuseFactor(int index, float3 normal)
			{
				float3 diffuseLight = MVisibleLightColors[index].rgb;
				float3 direction = MVisibleLightDirecitons[index].xyz;
				return saturate(dot(normalize(direction), normalize(normal))) * diffuseLight;
			}

			MVertexOut LegacyVertex(MVertexIn inVert)
			{
				UNITY_SETUP_INSTANCE_ID(inVert);
				MVertexOut vert;

				UNITY_TRANSFER_INSTANCE_ID(inVert, vert);

				vert.positionWS = mul(unity_ObjectToWorld, inVert.position);
				vert.positionCS = mul(unity_MatrixVP, float4(vert.positionWS,1.0));
				
				float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MTexture_ST);
				vert.uv = inVert.uv * baseST.xy + baseST.zw;
				vert.normal = UnityObjectToWorldNormal(inVert.normal);
				return vert;
			}

			float4 LegacyFragment(MVertexOut vert) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(vert);
				float4 texColor = tex2D(MTexture, vert.uv)*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MTint);
				Surface surface;
				surface.normal = normalize(vert.normal);
				surface.viewDirection = normalize(_WorldSpaceCameraPos - vert.positionWS);
				surface.color = texColor.rgb;
				surface.alpha = texColor.a;
				surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,MMetallic);
				surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MSmoothness);

				BRDF brdf = GetBRDF(surface);

				return float4(GetLighting(surface, brdf) ,surface.alpha);
			}

			ENDCG
		}
	}
}
