Shader "Computed/SimpleComputed"
{
	Properties
	{
		_Color("Base Color", Color) = (1,1,1,1)
		_WrapLight("Wrap Light", Range(0.0, 1.0)) = 0.5
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
				float2 uv : TEXCOORD0;
			};
			
			uniform StructuredBuffer<int> triangles;
			uniform StructuredBuffer<DataPerVertex> vertices;

			v2f vert (uint triangleIndex : SV_VertexID)
			{ 
				uint vertexIndex = triangles[triangleIndex];
				v2f o;

				o.vertex = UnityObjectToClipPos(vertices[vertexIndex].position);
				o.normal = mul(vertices[vertexIndex].normal, unity_WorldToObject);

				o.uv = vertices[vertexIndex].uv;

				return o;
			}

			fixed4 _Color;	
			float _WrapLight;

			fixed4 frag (v2f i) : SV_Target
			{
				float3 n = i.normal.xyz;
				float3 l = normalize(_WorldSpaceLightPos0);

				return max(0.0, dot(n, l) * (1 - _WrapLight) + _WrapLight) * _LightColor0 * _Color;
			}
			ENDCG
		}
	}
}
