Shader "Custom/ColorTextureSL" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }

		Pass {
			Lighting Off

			SetTexture[_MainTex] {
				constantColor[_Color]
				combine texture * constant
			}
		}
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }

		Pass {
			Lighting Off
			Blend One One

			SetTexture[_MainTex] {
				constantColor[_Color]
				combine texture * constant
			}
		}
	}

	FallBack "Diffuse"
}
