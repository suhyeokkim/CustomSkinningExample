#ifndef DATA_DEFINATION
#define DATA_DEFINATION

/*
*  per-vertex data layout
*/
struct VertexInfo
{
	float4 position;
	float4 normal;

	float4 weight;
	int4 index;

	float2 uv;
};

/*
*  render data per vertex
*/
struct RenderData
{
	float4 position;
	float4 normal;

	float2 uv;
};

#endif