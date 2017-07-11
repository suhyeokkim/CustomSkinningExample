// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TextureArray" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTexArray ("Albedo (RGB)", 2DArray) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 _Color;
			UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, i.uv);
				return col * _Color;
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
