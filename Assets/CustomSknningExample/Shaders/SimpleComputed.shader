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
			Name "ColorDraw"

			CGPROGRAM

			#pragma target 5.0

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "DataDefination.cginc"
	
			struct v2f
			{
				float4 positionCS	: SV_POSITION;
				float3 positionWS	: TEXCOORD1;	
				float2 uv			: TEXCOORD0;
			};
			
			uniform StructuredBuffer<int> triangles;
			uniform StructuredBuffer<DataPerVertex> dataPerVertex;

			v2f vert (uint vertexIndex : SV_VertexID)
			{ 
				DataPerVertex vertex = dataPerVertex[vertexIndex];
				v2f o;

				o.positionCS = UnityObjectToClipPos(vertex.position);
				o.positionWS = vertex.position;
				o.uv = vertex.uv;

				return o;
			}

			fixed4 _Color;	
			float _WrapLight;

			fixed4 frag (v2f i) : SV_Target
			{
				float3 n = cross(normalize(ddx(i.positionWS)), normalize(ddy(i.positionWS)));
				float3 l = normalize(_WorldSpaceLightPos0);
				float ndotl = max(min(dot(n, l), 1), 0);
				return max(0.0, ndotl * (1 - _WrapLight) + _WrapLight) * _LightColor0 * _Color;
			}
			ENDCG
		}

		Pass
		{
			Name "ShadowDraw"

			CGPROGRAM

			#pragma target 5.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "DataDefination.cginc"

			struct v2f
			{
				float4 positionCS	: SV_POSITION;
			};

			uniform StructuredBuffer<int> triangles;
			uniform StructuredBuffer<DataPerVertex> dataPerVertex;

			v2f vert(uint triangleIndex : SV_VertexID)
			{
				uint vertexIndex = triangles[triangleIndex];
				v2f o;

				o.positionCS = UnityObjectToClipPos(dataPerVertex[vertexIndex].position);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(1, 1, 1, 1);
			}
			ENDCG
		}
	}
}
