using System;
using Unity.Collections;
using UnityEngine;

public sealed class ChunkMeshNativeData : IDisposable
{
    public NativeArray<Vector3> Vertices;
    public NativeArray<Vector3> VerticesShadow;
    public NativeArray<int> Triangles;
    public NativeArray<Vector2> Uvs;
    public NativeArray<Vector3> Normals;
    public NativeArray<int> Count;

    public bool IsCreated => Vertices.IsCreated;

    public static ChunkMeshNativeData CreateFromJobOutput(
        NativeList<Vector3> vertices,
        NativeList<Vector3> verticesShadow,
        NativeList<int> triangles,
        NativeList<Vector2> uvs,
        NativeList<Vector3> normals,
        NativeList<int> count)
    {
        ChunkMeshNativeData data = new ChunkMeshNativeData
        {
            Vertices = new NativeArray<Vector3>(vertices.AsArray(), Allocator.Persistent),
            VerticesShadow = new NativeArray<Vector3>(verticesShadow.AsArray(), Allocator.Persistent),
            Triangles = new NativeArray<int>(triangles.AsArray(), Allocator.Persistent),
            Uvs = new NativeArray<Vector2>(uvs.AsArray(), Allocator.Persistent),
            Normals = new NativeArray<Vector3>(normals.AsArray(), Allocator.Persistent),
            Count = new NativeArray<int>(count.AsArray(), Allocator.Persistent)
        };

        return data;
    }

    public void Dispose()
    {
        if (Vertices.IsCreated) Vertices.Dispose();
        if (VerticesShadow.IsCreated) VerticesShadow.Dispose();
        if (Triangles.IsCreated) Triangles.Dispose();
        if (Uvs.IsCreated) Uvs.Dispose();
        if (Normals.IsCreated) Normals.Dispose();
        if (Count.IsCreated) Count.Dispose();
    }
}
