using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshExtension
{
    public static int GetTopologyCount(this Mesh mesh, int submesh)
    {
        MeshTopology topol = mesh.GetTopology(submesh);

        switch (topol)
        {
            case MeshTopology.Triangles:
                return 3;
            case MeshTopology.Quads:
                return 4;
            case MeshTopology.Lines:
            case MeshTopology.LineStrip:
                return 2;
            case MeshTopology.Points:
                return 1;
            default:
                return 0;
        }
    }
}

/// <summary>
/// dual quaternion translation extension 
/// </summary>
public static class DQExtension
{
    public static DualQuaternion GetLocalToWorldDQ(this Transform transform)
    {
        if (transform.parent != null)
            return GetLocalToWorldDQ(transform.parent) *
                new DualQuaternion(transform.localPosition) * new DualQuaternion(transform.localRotation);
        else
            return new DualQuaternion(transform.localPosition) * new DualQuaternion(transform.localRotation);
    }

    public static DualQuaternion GetWorldToLocalDQ(this Transform transform)
    {
        return transform.GetLocalToWorldDQ().inverse;
    }
}

public static class Matrix4x4Extension
{
    public static void Multiply(this Matrix4x4 matrix, float scalar)
    {
        for (int i = 0; i < 16; i++)
            matrix[i] *= scalar;
    }

    public static Vector3 ToTranslate(this Matrix4x4 matrix)
    {
        return new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
    }

    public static Quaternion ToRotation(this Matrix4x4 matrix)
    {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(1.0f + matrix.m00 + matrix.m11 + matrix.m22) / 2.0f;
        float w4 = (4.0f * q.w);
        q.x = (matrix.m21 - matrix.m12) / w4;
        q.y = (matrix.m02 - matrix.m20) / w4;
        q.z = (matrix.m10 - matrix.m01) / w4;

        return q;
    }

    public static DualQuaternion ToDQ(this Matrix4x4 matrix)
    {
        DualQuaternion dq = DualQuaternion.identity;
        return new DualQuaternion(matrix.ToRotation(), matrix.ToTranslate());
    }
}

public static class QuaternionExtension
{
    public static Quaternion Add(this Quaternion q1, Quaternion q2)
    {
        return new Quaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
    }

    public static Quaternion Normalize(this Quaternion q)
    {
        float len = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

        q.x /= len;
        q.y /= len;
        q.z /= len;
        q.w /= len;

        return q;
    }

    public static Quaternion Multiply(this Quaternion q, float s)
    {
        q.x *= s;
        q.y *= s;
        q.z *= s;
        q.w *= s;
        return q;
    }
}