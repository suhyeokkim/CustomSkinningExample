#ifndef QUATERNION_ARITHMETIC
#define QUATERNION_ARITHMETIC

float4 mulQxQ(float4 q1, float4 q2)
{
    return float4(
                    q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y,
                    q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x,
                    q1.w * q2.z - q1.x * q2.y - q1.y * q2.x + q1.z * q2.w,
                    q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z
                );
}

float4 conjugateQuaternion(float4 q)
{
    return float4(q.xyz * -1, q.w);
}

float3 transformPositionBy(float4 q, float3 p)
{
	// Extract the vector part of the quaternion
	float3 u = float3(q.x, q.y, q.z);

	// Extract the scalar part of the quaternion
	float s = q.w;

	// Do the math
	return 2.0 * dot(u, p) * u + (s * s - dot(u, u)) * p + 2.0 * s * cross(u, p);
}

#endif