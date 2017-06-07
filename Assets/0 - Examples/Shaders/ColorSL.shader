Shader "Custom/ColorSL"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Pass
		{
			Lighting Off

			SetTexture[_MainTex] {
				combine constant
			}
		}
	}
}
