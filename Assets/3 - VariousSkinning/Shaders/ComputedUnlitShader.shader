Shader "Custom/ComputedUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			uniform StructuredBuffer<int> triangles;
			uniform StructuredBuffer<float3> vertices;
			uniform StructuredBuffer<float2> uvs;

			sampler2D _MainTex;
			
			v2f vert (uint id : SV_VertexID)
			{
				int tri = triangles[id];
				v2f o;
				o.vertex = UnityObjectToClipPos(vertices[tri]);
				o.uv = uvs[tri];
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
