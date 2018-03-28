using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// dual quaternion data class
/// </summary>
[System.Serializable]
public struct DualQuaternion
{
    public Quaternion real;
    public Quaternion dual;

    public static DualQuaternion identity = new DualQuaternion(Quaternion.identity, Vector3.zero);

    public DualQuaternion(Quaternion real, Quaternion dual)
    {
        this.real = real;
        this.dual = dual;
    }

    public DualQuaternion(Quaternion rotation)
    {
        real = rotation;
        dual = new Quaternion(0f, 0f, 0f, 0f);
    }

    public DualQuaternion(Vector3 position)
    {
        real = Quaternion.identity;
        dual = (new Quaternion(position.x, position.y, position.z, 0) * real).Multiply(0.5f);
    }

    public DualQuaternion(Quaternion rotation, Vector3 position)
    {
        real = rotation;
        dual = (new Quaternion(position.x, position.y, position.z, 0) * rotation).Multiply(0.5f);
    }

    public Quaternion rotation { get { return real; } set { real = value; } }
    public Vector3 translate
    {
        get
        {
            Quaternion t = dual.Multiply(2f) * Quaternion.Inverse(real);
            return new Vector3(t.x, t.y, t.z);
        }
        set
        {
            dual = (new Quaternion(value.x, value.y, value.z, 0) * real).Multiply(0.5f);
        }
    }
    public DualQuaternion inverse { get { return Inverse(this); } }
    public DualQuaternion normalize { get { return Normalize(this); } }

    public static DualQuaternion Normalize(DualQuaternion dq)
    {
        float len = Mathf.Sqrt(dq.real.x * dq.real.x + dq.real.y * dq.real.y + dq.real.z * dq.real.z + dq.real.w * dq.real.w);
        dq.real = dq.real.Multiply(1 / len);
        dq.dual = dq.dual.Multiply(1 / len);
        return dq;
    }

    public static DualQuaternion Inverse(DualQuaternion dq)
    {
        float real = (dq.real.w * dq.real.w + dq.real.x * dq.real.x + dq.real.y * dq.real.y + dq.real.z * dq.real.z),
                dual = (dq.real.w * dq.dual.w + dq.real.x * dq.dual.x + dq.real.y * dq.dual.y + dq.real.z * dq.dual.z) * 2.0f;

        DualQuaternion other;

        other.real = Quaternion.Inverse(dq.real);
        other.dual = new Quaternion(dq.dual.x * (dual - real), dq.dual.y * (dual - real), dq.dual.z * (dual - real), dq.dual.w * (real - dual));

        return other.normalize;
    }

    public Matrix4x4 ToMatrix()
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        float
            xx = real.x * real.x, xy = real.x * real.y, xz = real.x * real.z, xw = real.x * real.w,
            yy = real.y * real.y, yz = real.y * real.z, yw = real.y * real.w,
            zz = real.z * real.z, zw = real.z * real.w;

        matrix[0, 0] = 1 - 2 * yy - 2 * zz;
        matrix[0, 1] = 2 * xy - 2 * zw;
        matrix[0, 2] = 2 * xz + 2 * yw;

        matrix[1, 0] = 2 * xy + 2 * zw;
        matrix[1, 1] = 1 - 2 * xx - 2 * zz;
        matrix[1, 2] = 2 * yz - 2 * xw;

        matrix[2, 0] = 2 * xz - 2 * yw;
        matrix[2, 1] = 2 * yz + 2 * xw;
        matrix[2, 2] = 1 - 2 * xx - 2 * yy;

        Vector3 trans = translate;

        matrix[0, 3] = trans.x;
        matrix[1, 3] = trans.y;
        matrix[2, 3] = trans.z;

        matrix[3, 0] = 0;
        matrix[3, 1] = 0;
        matrix[3, 2] = 0;
        matrix[3, 3] = 1;

        return matrix;
    }

    public static DualQuaternion operator *(DualQuaternion dq1, DualQuaternion dq2)
    {
        /*/
        DualQuaternion dq;

        dq.real.w = dq1.real.w * dq2.real.w - dq1.real.x * dq2.real.x - dq1.real.y * dq2.real.y - dq1.real.z * dq2.real.z;
        dq.real.x = dq1.real.w * dq2.real.x + dq1.real.x * dq2.real.w + dq1.real.y * dq2.real.z - dq1.real.z * dq2.real.y;
        dq.real.y = dq1.real.w * dq2.real.y + dq1.real.y * dq2.real.w - dq1.real.x * dq2.real.z + dq1.real.z * dq2.real.x;
        dq.real.z = dq1.real.w * dq2.real.z + dq1.real.z * dq2.real.w + dq1.real.x * dq2.real.y - dq1.real.y * dq2.real.x;

        dq.dual.x = dq1.dual.x * dq2.real.w + dq1.real.w * dq2.dual.x + dq1.dual.w * dq2.real.x + dq1.real.x * dq2.dual.w -
                    dq1.dual.z * dq2.real.y + dq1.real.y * dq2.dual.z + dq1.dual.y * dq2.real.z - dq1.real.z * dq2.dual.y;
        dq.dual.y = dq1.dual.y * dq2.real.w + dq1.real.w * dq2.dual.y + dq1.dual.z * dq2.real.x - dq1.real.x * dq2.dual.z +
                    dq1.dual.w * dq2.real.y + dq1.real.y * dq2.dual.w - dq1.dual.x * dq2.real.z + dq1.real.z * dq2.dual.x;
        dq.dual.z = dq1.dual.z * dq2.real.w + dq1.real.w * dq2.dual.z - dq1.dual.y * dq2.real.x + dq1.real.x * dq2.dual.y +
                    dq1.dual.x * dq2.real.y - dq1.real.y * dq2.dual.x + dq1.dual.w * dq2.real.z + dq1.real.z * dq2.dual.w;
        dq.dual.w = dq1.dual.w * dq2.real.w + dq1.real.w * dq2.dual.w - dq1.real.x * dq2.dual.x - dq1.dual.x * dq2.real.x -
                    dq1.real.y * dq2.dual.y - dq1.dual.y * dq2.real.y - dq1.real.z * dq2.dual.z - dq1.dual.z * dq2.real.z;

        return dq;
        /*/
        return new DualQuaternion(
            (dq1.real * dq2.real), 
            (dq1.dual * dq2.real).Add(dq1.real * dq2.dual)
            ).normalize;
        //*/
    }

    public static Vector3 operator *(DualQuaternion dq, Vector3 pos)
    {
        return dq.real * pos + dq.translate;
    }

    public static bool operator ==(DualQuaternion dq1, DualQuaternion dq2)
    {
        return dq1.real.Equals(dq2.real) && dq1.dual.Equals(dq2.dual);
    }

    public static bool operator !=(DualQuaternion dq1, DualQuaternion dq2)
    {
        return !dq1.real.Equals(dq2.real) || !dq1.dual.Equals(dq2.dual);
    }

    public override bool Equals(object obj)
    {
        if (obj is DualQuaternion)
        {
            DualQuaternion dq = (DualQuaternion)obj;
            return dq == this;
        }
        else
            return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format("real : {0}, dual : {1}", real.ToString("F5"), dual.ToString("F5"));
    }
}
