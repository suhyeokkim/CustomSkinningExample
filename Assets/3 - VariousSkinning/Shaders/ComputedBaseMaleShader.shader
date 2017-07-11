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
			Cull Back

			CGPROGRAM

			#pragma target 5.0

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct RenderData
			{
				float4 position;
				float4 normal;

				float2 uv;
			};

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
				return _Color;
			}
			ENDCG
		}
	}
}
