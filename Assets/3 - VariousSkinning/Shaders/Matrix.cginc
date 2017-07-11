#ifndef QUATERNION_ARITHMETIC
#define QUATERNION_ARITHMETIC

inline float3x3 quaternionToMatrix(float4 q)
{
	float3x3 rotationMatrix;

	return rotationMatrix;
}

inline float4x4 matrixFromTRS(float3 translate, float4 rotation, float3 scale)
{
	float4x4 transformMatrix;

	return transformMatrix;
}

inline float3 translateFromMatrix(float4x4 transformMatrix)
{
	float3 translate;

	return translate;
}

inline float4 rotationFromMatrix(float4x4 transformMatrix)
{
	float4 rotation;

	return rotation;	
}

#endif