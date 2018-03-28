#ifndef DATA_DEFINATION
#define DATA_DEFINATION

struct DataPerVertex
{
	float4 position;
	float4 normal;
	float4 tangent;

	float2 uv;
};

struct SkinPerVertex
{
	float4 weight;	
	int4 index;
};

#endif