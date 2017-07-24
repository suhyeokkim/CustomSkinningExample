Shader "Custom/ComputedAdamShader"
{
	Properties
	{
		_MainTexArray ("Albedo (RGB)", 2DArray) = "white" {}
		_SpeclarArray("Specular", 2DArray) = "white" {}
		_BumpArray("Normal", 2DArray) = "white" {}
		_OcclusionArray("Ambient Occlusion", 2DArray) = "white" {}
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

			UNITY_DECLARE_TEX2DARRAY(_MainTexArray);
			UNITY_DECLARE_TEX2DARRAY(_SpeclarArray);
			UNITY_DECLARE_TEX2DARRAY(_BumpArray);
			UNITY_DECLARE_TEX2DARRAY(_OcclusionArray);

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
