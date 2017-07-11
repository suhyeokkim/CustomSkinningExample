using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Rigger
{
    public static void GetBindPoses(ref Matrix4x4[] bindPoses, Transform root, Transform[] bones)
    {
        if (bindPoses == null)
            bindPoses = new Matrix4x4[bones.Length];
        else if (bindPoses.Length < bones.Length)
            Array.Resize(ref bindPoses, bones.Length);

        for (int i = 0; i < bones.Length; i++)
            bindPoses[i] = bones[i].worldToLocalMatrix * root.localToWorldMatrix;
    }
}
