Shader "Computed/SimpleTentionComputed"
{
	Properties
	{
		_Color("Base Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma target 5.0

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ TENSION_DEBUG

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "DataDefination.cginc"

			struct VS_OUTPUT
			{
				float4 vertex		: SV_POSITION;
				float3 normal		: NORMAL;
				float3 tangent		: TANGENT;
				float2 uv			: TEXCOORD0;
#if TENSION_DEBUG
				float3 color		: COLOR;
#endif
			};

			uniform StructuredBuffer<int> triangles;
			uniform StructuredBuffer<DataPerVertex> dataPerVertex;
#if TENSION_DEBUG
			uniform Buffer<float> tension;
#endif

			VS_OUTPUT vert(uint triangleIndex : SV_VertexID)
			{
				uint vertexIndex = triangles[triangleIndex];
				VS_OUTPUT o;

				o.vertex = UnityObjectToClipPos(dataPerVertex[vertexIndex].position);
				o.normal = mul(unity_WorldToObject, dataPerVertex[vertexIndex].normal);
				o.tangent = mul(unity_WorldToObject, dataPerVertex[vertexIndex].tangent);
				o.uv = dataPerVertex[vertexIndex].uv;

#if TENSION_DEBUG
				if (tension[vertexIndex] > 1)
					o.color = float3(0, 1, 0) * (tension[vertexIndex] - 1);
				else
					o.color = float3(1, 0, 0) * (1 - tension[vertexIndex]);
#endif

				return o;
			}

			fixed4 _Color;

			fixed4 frag(VS_OUTPUT i) : SV_Target
			{
#if TENSION_DEBUG
				return fixed4(i.color, 0);
#else 
				float3 n = i.normal.xyz;
				float3 l = normalize(_WorldSpaceLightPos0);

				return max(dot(n, l), 0) * _LightColor0 * _Color;
#endif
			}
			ENDCG
		}
	}
}
