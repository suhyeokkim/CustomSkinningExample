#ifndef QUATERNION_ARITHMETIC
#define QUATERNION_ARITHMETIC

// UnityEngine.Quaternion operator*
float4 mulQxQ(float4 lhs, float4 rhs)
{
    return float4(
                    lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                    lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                    lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                    lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z
                );
}

float4 conjugateQuaternion(float4 q)
{
    return float4(q.xyz * -1.0, q.w);
}

// https://gamedev.stackexchange.com/questions/28395/rotating-vector3-by-a-quaternion
float3 transformPositionByQ(float4 q, float3 p)
{
	float3 u = float3(q.x, q.y, q.z);
	float s = q.w;
	return 2.0 * dot(u, p) * u + (s * s - dot(u, u)) * p + 2.0 * s * cross(u, p);
}

inline float3x3 quaternionToMatrix(float4 q)
{
	float3x3 rotationMatrix;

	return rotationMatrix;
}

#endif