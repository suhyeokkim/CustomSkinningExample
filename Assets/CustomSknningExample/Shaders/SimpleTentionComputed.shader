// hull shader reference : https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/
Shader "Computed/SimpleTentionComputed"
{
	Properties
	{
		_Color("Base Color", Color) = (1,1,1,1)
		_WrapLight("Wrap Light", Range(0.0, 1.0)) = 0.5
		_EdgeLength("Unit Edge Length(Tesselation)", Range(0.1, 1)) = 0.5
		_InsideDevide("Unit Devide(Tesselation)", Range(1, 64)) = 1
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma target 5.0

			#pragma vertex vert
			#pragma hull hull
			#pragma domain domain
			#pragma fragment frag

			#pragma multi_compile _ TENSION_DEBUG

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "DataDefination.cginc"

			struct vout
			{
				float4 positionWS	: INTERNALTESSPOS;
				float2 uv			: TEXCOORD0;
			};

			uniform StructuredBuffer<DataPerVertex> dataPerVertex;

			vout vert(uint vertexIndex : SV_VertexID)
			{
				DataPerVertex vertex = dataPerVertex[vertexIndex];
				vout o;

				o.positionWS = dataPerVertex[vertexIndex].position;
				o.uv = dataPerVertex[vertexIndex].uv;

				return o;
			}

			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("HullPatchConstant")]
			vout hull(InputPatch<vout, 3> patch, uint controlPointID : SV_OutputControlPointID, uint primID : SV_PrimitiveID)
			{
				return patch[controlPointID];
			}

			struct hullFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};
			
			float _EdgeLength, _InsideDevide;

			float HullEdgefactor(float3 p0, float p1) 
			{
				float edgeLength = distance(p0, p1);

				float3 edgeCenter = (p0 + p1) * 0.5;
				float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);

				return edgeLength * _ScreenParams.y / (_EdgeLength * viewDistance);
			}
			hullFactors HullPatchConstant(InputPatch<vout, 3> patch)
			{
				float3	p0 = patch[0].positionWS, 
						p1 = patch[1].positionWS,
						p2 = patch[2].positionWS;
				hullFactors f;
				f.edge[0] = HullEdgefactor(p1, p2);
				f.edge[1] = HullEdgefactor(p2, p0);
				f.edge[2] = HullEdgefactor(p0, p1);
				f.inside = (HullEdgefactor(p1, p2) + HullEdgefactor(p2, p0) + HullEdgefactor(p0, p1)) * (1 / 3.0);
				return f;
			}

			struct dout
			{
				float4 positionCS	: SV_Position;
				float3 positionWS	: TEXCOORD1;
				float2 uv			: TEXCOORD0;
			};
			[UNITY_domain("tri")]
			dout domain(hullFactors factors, OutputPatch<vout, 3> patch, float3 bc : SV_DomainLocation)
			{
				dout v;
				v.positionWS = patch[0].positionWS * bc.x + patch[1].positionWS * bc.y + patch[2].positionWS * bc.z;
				v.uv = patch[0].uv * bc.x + patch[1].uv * bc.y + patch[2].uv * bc.z;
				v.positionCS = UnityObjectToClipPos(v.positionWS);
				return v;
			}

			fixed4 _Color;
			float _WrapLight;

			fixed4 frag(dout i) : SV_Target
			{
				float3 n = cross(normalize(ddx(i.positionWS)), normalize(ddy(i.positionWS)));
				float3 l = normalize(_WorldSpaceLightPos0);
				float ndotl = max(min(dot(n, l), 1), 0);
				return max(0.0, ndotl * (1 - _WrapLight) + _WrapLight) * _LightColor0 * _Color;
			}
			ENDCG
		}
	}
}
