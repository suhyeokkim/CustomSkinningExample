Shader "Custom/TextureSL" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader {
		Pass {
			Lighting Off

			SetTexture[_MainTex] {
				combine texture
			}
		}
	}
}
