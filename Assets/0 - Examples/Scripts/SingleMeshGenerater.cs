using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModifyMeshEvent : UnityEngine.Events.UnityEvent<Mesh> { }

/// <summary>
/// pick mesh in meshfilter and pass mesh to event.
/// </summary>
public class SingleMeshGenerater : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter filter;

    [SerializeField]
    private Material material;

    [SerializeField]
    public ModifyMeshEvent modifyEvent;

    void Generate()
    {
        filter = GetComponent<MeshFilter>();

        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = material;

        Mesh mesh = filter.sharedMesh == null ? new Mesh() : filter.sharedMesh;

        if (modifyEvent != null)
            modifyEvent.Invoke(mesh);

        filter.sharedMesh = mesh;
    }

    void Awake()
    {
        Generate();
    }
}
