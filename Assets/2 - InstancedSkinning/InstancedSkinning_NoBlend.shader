Shader "Custom/InstancedSkinning_NoBlend" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTexArray ("Albedo (RGB)", 2DArray) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};
			 
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
			};

			// i cant find about use instanceID.(known as SV_InstanceID or GL_InstanceID) and i just declare few things which is bone variable.
			// so i upload question at Unity3D Answer site : http://answers.unity3d.com/questions/1362556/how-to-approach-instanceid-in-shader.html
			// if you want method for approaching instanceID, confirm ↑ this url..

			UNITY_INSTANCING_CBUFFER_START(_BoneProperties)
				float4 _BonePosition0;
				float4 _BonePosition1;
				float4 _BonePosition2;
				float4 _BonePosition3;
				float4 _BonePosition4;
				float4 _BonePosition5;
				float4x4 _BoneTransfromMatrix0;
				float4x4 _BoneTransfromMatrix1;
				float4x4 _BoneTransfromMatrix2;
				float4x4 _BoneTransfromMatrix3;
				float4x4 _BoneTransfromMatrix4;
				float4x4 _BoneTransfromMatrix5;
			UNITY_INSTANCING_CBUFFER_END
			
			float4 GetPosition(uint index)
			{
				switch(index)
				{
					case 0:
						return _BonePosition0;
					case 1:
						return _BonePosition1;
					case 2:
						return _BonePosition2;
					case 3:
						return _BonePosition3;
					case 4:
						return _BonePosition4;
					case 5:
						return _BonePosition5;
				}

				return _BonePosition0;
			}

			float4x4 GetMatrix(uint index)
			{
				switch(index)
				{
					case 0:
						return _BoneTransfromMatrix0;
					case 1:
						return _BoneTransfromMatrix1;
					case 2:
						return _BoneTransfromMatrix2;
					case 3:
						return _BoneTransfromMatrix3;
					case 4:
						return _BoneTransfromMatrix4;
					case 5:
						return _BoneTransfromMatrix5;
				}

				return _BoneTransfromMatrix0;
			}

			v2f vert (appdata v)
			{
				v2f o;
				
				uint boneIndex = v.uv[3];

				float4 pos = GetPosition(boneIndex);

				o.vertex = UnityObjectToClipPos(
								mul(
									GetMatrix(boneIndex),
									float4(v.vertex.xyz - pos.xyz,1)
								)
								+
								float4(pos.xyz, 0)
							);
				o.uv = v.uv;
				
				return o;
			}
			
			UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, i.uv);
				return col;// * UNITY_ACCESS_INSTANCED_PROP(_Color);
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
