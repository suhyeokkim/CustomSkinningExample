#ifndef DUALQUATERNION_ARTHIMETIC
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#define DUALQUATERNION_ARTHIMETIC

#ifndef QUATERNION_ARITHMETIC
#   include <Quaternion.cginc>
#endif

struct DQ
{
	float4 real;
	float4 dual;
};

inline DQ multiply(DQ dq, float scalar)
{
	dq.real *= scalar;
	dq.dual *= scalar;

	return dq;
}

inline DQ add(DQ dq1, DQ dq2)
{
	DQ dq;

	dq.real = dq1.real + dq2.real;
	dq.dual = dq1.dual + dq2.dual;

	return dq;
}

inline DQ minus(DQ dq1, DQ dq2)
{
	DQ dq;

	dq.real = dq1.real - dq2.real;
	dq.dual = dq1.dual - dq2.dual;

	return dq;
}

DQ mulDQ(DQ dq1, DQ dq2)
{
	DQ dq;

	dq.real = mulQxQ(dq1.real, dq2.real);
	dq.dual = mulQxQ(dq1.dual, dq2.real) + mulQxQ(dq1.real, dq2.dual);

	return dq;
}

float2x4 mulDQ(float2x4 dq1, float2x4 dq2)
{
	float2x4 dq;

	dq[0] = mulQxQ(dq1[0], dq2[0]);
	dq[1] = mulQxQ(dq1[1], dq2[0]) + mulQxQ(dq1[0], dq2[1]);

	return dq;
}

float3 translateFromDQ(DQ dq)
{
	return
		mulQxQ(
			dq.dual * 2,
			conjugateQuaternion(dq.real)
		).xyz;
}

float3 translateFromDQ(float2x4 dq)
{
	return
		mulQxQ(
			dq[1] * 2,
			conjugateQuaternion(dq[0])
		).xyz;
}

float3 transformPositionByDQ(DQ dq, float3 pos)
{
	return translateFromDQ(dq) + mulQxQ(dq.real, float4(pos, 0)).xyz;
}

float3 transformPositionByDQ(float2x4 dq, float3 pos)
{
	return translateFromDQ(dq) + mulQxQ(dq[0], float4(pos, 0)).xyz;
}

float4x4 DQToMatrix(DQ dq)
{
	float4x4 convetedMatrix;
	float   xx = dq.real.x * dq.real.x, xy = dq.real.x * dq.real.y, xz = dq.real.x * dq.real.z, xw = dq.real.x * dq.real.w,
		yy = dq.real.y * dq.real.y, yz = dq.real.y * dq.real.z, yw = dq.real.y * dq.real.w,
		zz = dq.real.z * dq.real.z, zw = dq.real.z * dq.real.w;

	convetedMatrix[0][0] = 1 - 2 * yy - 2 * zz;
	convetedMatrix[0][1] = 2 * xy - 2 * zw;
	convetedMatrix[0][2] = 2 * xz + 2 * yw;

	convetedMatrix[1][0] = 2 * xy + 2 * zw;
	convetedMatrix[1][1] = 1 - 2 * xx - 2 * zz;
	convetedMatrix[1][2] = 2 * yz - 2 * xw;

	convetedMatrix[2][0] = 2 * xz - 2 * yw;
	convetedMatrix[2][1] = 2 * yz + 2 * xw;
	convetedMatrix[2][2] = 1 - 2 * xx - 2 * yy;

	float3 trans = translateFromDQ(dq);

	convetedMatrix[0][3] = trans.x;
	convetedMatrix[1][3] = trans.y;
	convetedMatrix[2][3] = trans.z;

	convetedMatrix[3][0] = 0;
	convetedMatrix[3][1] = 0;
	convetedMatrix[3][2] = 0;
	convetedMatrix[3][3] = 1;

	return convetedMatrix;
}

float4x4 DQToMatrix(float2x4 dq)
{
	float4x4 convetedMatrix;
	float   xx = dq[0].x * dq[0].x, xy = dq[0].x * dq[0].y, xz = dq[0].x * dq[0].z, xw = dq[0].x * dq[0].w,
		yy = dq[0].y * dq[0].y, yz = dq[0].y * dq[0].z, yw = dq[0].y * dq[0].w,
		zz = dq[0].z * dq[0].z, zw = dq[0].z * dq[0].w;

	convetedMatrix[0][0] = 1 - 2 * yy - 2 * zz;
	convetedMatrix[0][1] = 2 * xy - 2 * zw;
	convetedMatrix[0][2] = 2 * xz + 2 * yw;

	convetedMatrix[1][0] = 2 * xy + 2 * zw;
	convetedMatrix[1][1] = 1 - 2 * xx - 2 * zz;
	convetedMatrix[1][2] = 2 * yz - 2 * xw;

	convetedMatrix[2][0] = 2 * xz - 2 * yw;
	convetedMatrix[2][1] = 2 * yz + 2 * xw;
	convetedMatrix[2][2] = 1 - 2 * xx - 2 * yy;

	float3 trans = translateFromDQ(dq);

	convetedMatrix[0][3] = trans.x;
	convetedMatrix[1][3] = trans.y;
	convetedMatrix[2][3] = trans.z;

	convetedMatrix[3][0] = 0;
	convetedMatrix[3][1] = 0;
	convetedMatrix[3][2] = 0;
	convetedMatrix[3][3] = 1;

	return convetedMatrix;
}
#endif
