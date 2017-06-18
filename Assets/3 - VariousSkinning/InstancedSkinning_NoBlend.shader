Shader "Custom/InstancedSkinning_NoBlend" {
	Properties {
		_MainTexArray ("Albedo (RGB)", 2DArray) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma enable_d3d11_debug_symbols
			#pragma exclude_renderers d3d9 gles d3d11_9x
			#pragma only_renderers d3d11 glcore gles3 metal vulkan

			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct a2v
			{
				float3 uv : TEXCOORD0;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			  
			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
			};

			#define UNITY_MAX_INSTANCE_COUNT 100

			UNITY_INSTANCING_CBUFFER_START(_BonePositions)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BonePosition0);
				UNITY_DEFINE_INSTANCED_PROP(float4, _BonePosition1);
				UNITY_DEFINE_INSTANCED_PROP(float4, _BonePosition2);
				UNITY_DEFINE_INSTANCED_PROP(float4, _BonePosition3);
				UNITY_DEFINE_INSTANCED_PROP(float4, _BonePosition4);
				UNITY_DEFINE_INSTANCED_PROP(float4, _BonePosition5);
			UNITY_INSTANCING_CBUFFER_END
			
			float4 GetPosition(uint index)
			{
				switch(index)
				{
					case 0:
						return UNITY_ACCESS_INSTANCED_PROP(_BonePosition0);
					case 1:
						return UNITY_ACCESS_INSTANCED_PROP(_BonePosition1);
					case 2:
						return UNITY_ACCESS_INSTANCED_PROP(_BonePosition2);
					case 3:
						return UNITY_ACCESS_INSTANCED_PROP(_BonePosition3);
					case 4:
						return UNITY_ACCESS_INSTANCED_PROP(_BonePosition4);
					case 5:
						return UNITY_ACCESS_INSTANCED_PROP(_BonePosition5);
				}

				return float4(1, 1, 1, 1);
			}
			
			#define UNITY_MAX_INSTANCE_COUNT 100

			UNITY_INSTANCING_CBUFFER_START(_BoneMatrixs)
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _BoneTransfromMatrix0);
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _BoneTransfromMatrix1);
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _BoneTransfromMatrix2);
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _BoneTransfromMatrix3);
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _BoneTransfromMatrix4);
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _BoneTransfromMatrix5);
			UNITY_INSTANCING_CBUFFER_END

			float4x4 GetMatrix(uint index)
			{
				switch(index)
				{
					case 0:
						return UNITY_ACCESS_INSTANCED_PROP(_BoneTransfromMatrix0);
					case 1:
						return UNITY_ACCESS_INSTANCED_PROP(_BoneTransfromMatrix1);
					case 2:
						return UNITY_ACCESS_INSTANCED_PROP(_BoneTransfromMatrix2);
					case 3:
						return UNITY_ACCESS_INSTANCED_PROP(_BoneTransfromMatrix3);
					case 4:
						return UNITY_ACCESS_INSTANCED_PROP(_BoneTransfromMatrix4);
					case 5:
						return UNITY_ACCESS_INSTANCED_PROP(_BoneTransfromMatrix5);
				}

				return UNITY_MATRIX_MVP;
			}

			v2f vert (a2v v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);

				uint boneIndex = v.uv[2];

				float4 pos = GetPosition(boneIndex);

				o.vertex = UnityObjectToClipPos(
								mul(
									GetMatrix(boneIndex),
									float4(v.vertex.xyz - pos.xyz,1)
								)
								+
								float4(pos.xyz, 0)
							);
				o.uv = v.uv.xy;
				
				return o;
			}

			UNITY_INSTANCING_CBUFFER_START(_FragmentBuffer)
				float _TextureIndex;
			UNITY_INSTANCING_CBUFFER_END

			UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

			fixed4 frag (v2f i) : SV_Target
			{ 
				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(i.uv, _TextureIndex));
				return col;
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
