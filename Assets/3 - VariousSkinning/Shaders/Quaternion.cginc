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
	//float3 u = float3(q.x, q.y, q.z);
	//float s = q.w;
	//return 2.0 * dot(u, p) * u + (s * s - dot(u, u)) * p + 2.0 * s * cross(u, p);

	float num = q.x * 2;
	float num2 = q.y * 2;
	float num3 = q.z * 2;
	float num4 = q.x * num;
	float num5 = q.y * num2;
	float num6 = q.z * num3;
	float num7 = q.x * num2;
	float num8 = q.x * num3;
	float num9 = q.y * num3;
	float num10 = q.w * num;
	float num11 = q.w * num2;
	float num12 = q.w * num3;

	float3 result;
	result.x = (1 - (num5 + num6)) * p.x + (num7 - num12) * p.y + (num8 + num11) * p.z;
	result.y = (num7 + num12) * p.x + (1 - (num4 + num6)) * p.y + (num9 - num10) * p.z;
	result.z = (num8 - num11) * p.x + (num9 + num10) * p.y + (1 - (num4 + num5)) * p.z;
	return result;
}

inline float3x3 quaternionToMatrix(float4 q)
{
	float3x3 rotationMatrix;

	return rotationMatrix;
}

#endif