// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/ComputedBaseMaleShader"
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
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "DataDefination.cginc"
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 uv : TEXCOORD0;
			};
			
			uniform StructuredBuffer<int> triangles;
			uniform StructuredBuffer<RenderData> vertices;

			uniform StructuredBuffer<uint> triCountPerTextureIndex;

			inline int GetTextureIndex(uint triangleIndex)
			{
				int index;
				for (index = 0; triangleIndex < triCountPerTextureIndex[index]; index++);
				return index;
			}

			v2f vert (uint triangleIndex : SV_VertexID)
			{ 
				uint vertexIndex = triangles[triangleIndex];
				v2f o;

				o.vertex = UnityObjectToClipPos(vertices[vertexIndex].position);
				o.normal = vertices[vertexIndex].normal;

				o.uv = float3(vertices[vertexIndex].uv, GetTextureIndex(triangleIndex));

				return o;
			}

			fixed4 _Color;	

			fixed4 frag (v2f i) : SV_Target
			{
				float4 normal = float4(i.normal, 0.0);
				float3 n = normalize(mul(normal, unity_WorldToObject));
				float3 l = normalize(_WorldSpaceLightPos0);

				return saturate(max(0.0, dot(n, l)) * _LightColor0 * _Color);
			}
			ENDCG
		}
	}
}
