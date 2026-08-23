using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapMarker
{
    public ID id;
    public int x, z;
    public int y;
}

[Serializable]
public class WorldMap
{
    /// <summary>Per-column explored flag, indexed z * width + x. 0 = fog, 1 = revealed.</summary>
    public byte[] Explored;
    public List<MapMarker> Markers = new List<MapMarker>();

    [NonSerialized] public Texture2D Texture;
    [NonSerialized] public bool Dirty = true;
    [NonSerialized] private bool _markersBuilt;
    [NonSerialized] public bool FullReveal;
    [NonSerialized] private byte[] _savedExplored;

    private static readonly Color FogColor = new Color(0.08f, 0.08f, 0.10f, 1f);
    private static readonly Color VoidColor = new Color(0.75f, 0.75f, 0.85f, 1f);

    public void Reveal(int x, int z, int radius)
    {
        World world = World.Inst;
        int width = world.Bounds.x;
        int height = world.Bounds.z;
        if (Explored == null || Explored.Length != width * height) return;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dz = -radius; dz <= radius; dz++)
            {
                if (dx * dx + dz * dz > radius * radius) continue;
                int wx = x + dx;
                int wz = z + dz;
                if (wx < 0 || wx >= width || wz < 0 || wz >= height) continue;
                int idx = wz * width + wx;
                if (Explored[idx] == 0)
                {
                    Explored[idx] = 1;
                    Dirty = true;
                }
            }
        }
    }

    public void ToggleFullReveal()
    {
        World world = World.Inst;
        int width = world.Bounds.x;
        int height = world.Bounds.z;
        if (Explored == null || Explored.Length != width * height) return;

        if (!FullReveal)
        {
            _savedExplored = (byte[])Explored.Clone();
            for (int i = 0; i < Explored.Length; i++) Explored[i] = 1;
            FullReveal = true;
        }
        else
        {
            if (_savedExplored != null)
                _savedExplored.CopyTo(Explored, 0);
            FullReveal = false;
        }
        Dirty = true;
    }

    private const int SurfaceBand = 8;

    /// <summary>Force markers to rebuild on the next map update (called when a
    /// structure is placed or removed).</summary>
    public void ResetMarkers() => _markersBuilt = false;

    public void BuildMarkers(World world)
    {
        if (_markersBuilt) return;
        Markers.Clear();

        for (int cx = 0; cx < world.Size.x; cx++)
        {
            for (int cz = 0; cz < world.Size.z; cz++)
            {
                for (int cy = 0; cy < world.Size.y; cy++)
                {
                    Chunk chunk = world[cx * World.ChunkSize, cy * World.ChunkSize, cz * World.ChunkSize];
                    if (chunk == null) continue;
                    foreach (Info info in chunk.StaticEntity)
                        AddMarker(world, info);
                }
            }
        }

        _markersBuilt = true;
    }

    private void AddMarker(World world, Info info)
    {
        if (info == null) return;
        int x = (int)info.position.x;
        int z = (int)info.position.z;
        int y = (int)info.position.y;

        int surfaceY = FindSurfaceY(world, x, z);
        if (surfaceY < 0 || Mathf.Abs(y - surfaceY) > SurfaceBand) return;

        Markers.Add(new MapMarker { id = info.id, x = x, z = z, y = y });
    }

    private static int FindSurfaceY(World world, int x, int z)
    {
        for (int y = world.Bounds.y - 1; y >= 1; y--)
        {
            Vector3Int block = new Vector3Int(x, y, z);
            Vector3Int chunkCoord = World.GetChunkCoordinate(block);
            Chunk chunk = world[chunkCoord];
            if (chunk == null || chunk == Chunk.Zero) continue;

            int localX = block.x - chunkCoord.x;
            int localY = block.y - chunkCoord.y;
            int localZ = block.z - chunkCoord.z;
            if (localY == 0) continue;

            if (chunk[localX, localY, localZ] == 0 && chunk[localX, localY - 1, localZ] != 0)
                return y;
        }
        return -1;
    }

    public void RegenerateTexture(World world)
    {
        int width = world.Bounds.x;
        int height = world.Bounds.z;
        if (Explored == null || Explored.Length != width * height)
            Explored = new byte[width * height];
        if (Texture == null || Texture.width != width || Texture.height != height)
        {
            if (Texture != null) UnityEngine.Object.Destroy(Texture);
            Texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
        }

        Color[] pixels = new Color[width * height];
        int[] surfaceHeights = new int[width * height];

        for (int wz = 0; wz < height; wz++)
        {
            int cz = wz / World.ChunkSize;
            int lz = wz % World.ChunkSize;
            for (int wx = 0; wx < width; wx++)
            {
                int idx = wz * width + wx;
                if (Explored[idx] == 0)
                {
                    pixels[idx] = FogColor;
                    continue;
                }

                int cx = wx / World.ChunkSize;
                int lx = wx % World.ChunkSize;
                int block = 0;    // block to display (one below the topmost solid)
                int surfaceY = 0;
                int topBlock = 0; // topmost solid, used only if nothing sits beneath it
                int topY = 0;
                for (int cy = world.Size.y - 1; cy >= 0 && block == 0; cy--)
                {
                    Chunk chunk = world[cx * World.ChunkSize, cy * World.ChunkSize, cz * World.ChunkSize];
                    if (chunk == null) continue;
                    for (int ly = World.ChunkSize - 1; ly >= 0; ly--)
                    {
                        int b = chunk[lx, ly, lz];
                        if (b == 0) continue;
                        int wy = cy * World.ChunkSize + ly;
                        if (topBlock == 0) { topBlock = b; topY = wy; }
                        else { block = b; surfaceY = wy; break; }
                    }
                }
                if (block == 0) { block = topBlock; surfaceY = topY; }

                surfaceHeights[idx] = surfaceY;
                pixels[idx] = block != 0 ? GetBlockColor(block) : VoidColor;
            }
        }

        const int ShadowHeight = 12;
        const float ShadowStrength = 0.45f;
        for (int wz = 0; wz < height; wz++)
        {
            for (int wx = 0; wx < width; wx++)
            {
                int idx = wz * width + wx;
                if (Explored[idx] == 0) continue;

                float shadow = 0f;
                for (int d = 1; d <= 2; d++)
                {
                    int hx = wx + d;
                    int hz = wz + d;
                    if (hx >= width || hz >= height) break;
                    int hidx = hz * width + hx;
                    if (Explored[hidx] == 0) continue;
                    int diff = surfaceHeights[hidx] - surfaceHeights[idx];
                    if (diff > 0)
                        shadow = Mathf.Max(shadow, Mathf.Clamp01(diff / (float)ShadowHeight));
                }

                if (shadow > 0f)
                    pixels[idx] = Color.Lerp(pixels[idx], Color.black, shadow * ShadowStrength);
            }
        }

        const int EdgeWidth = 4;
        for (int wz = 0; wz < height; wz++)
        {
            for (int wx = 0; wx < width; wx++)
            {
                int idx = wz * width + wx;
                if (Explored[idx] == 0) continue;

                int dist = EdgeWidth;
                for (int d = 1; d <= EdgeWidth; d++)
                {
                    bool nearFog =
                        (wx - d >= 0 && Explored[wz * width + (wx - d)] == 0) ||
                        (wx + d < width && Explored[wz * width + (wx + d)] == 0) ||
                        (wz - d >= 0 && Explored[(wz - d) * width + wx] == 0) ||
                        (wz + d < height && Explored[(wz + d) * width + wx] == 0);
                    if (nearFog)
                    {
                        dist = d;
                        break;
                    }
                }

                if (dist < EdgeWidth)
                {
                    float t = 1f - (dist / (float)EdgeWidth);
                    pixels[idx] = Color.Lerp(pixels[idx], FogColor, t);
                }
            }
        }

        Texture.SetPixels(pixels);
        Texture.Apply();
        Dirty = false;
    }

    private static Color GetBlockColor(int blockID)
    {
        if (blockID == 0) return VoidColor;
        switch (Block.ConvertID(blockID))
        {
            case ID.GrassBlock: return new Color(0.60f, 0.64f, 0.42f);
            case ID.ForestBlock: return new Color(0.30f, 0.40f, 0.22f);
            case ID.SandBlock: return new Color(0.80f, 0.76f, 0.60f);
            case ID.StoneBlock: return new Color(0.55f, 0.55f, 0.55f);
            case ID.GraniteBlock: return new Color(0.62f, 0.58f, 0.58f);
            case ID.MarbleBlock: return new Color(0.72f, 0.76f, 0.82f);
            case ID.BrickBlock: return new Color(0.62f, 0.55f, 0.50f);
            case ID.WoodBlock: return new Color(0.48f, 0.40f, 0.30f);
            case ID.BackroomBlock: return new Color(0.62f, 0.60f, 0.42f);
            case ID.MulchBlock: return new Color(0.38f, 0.32f, 0.26f);
            default: return new Color(0.45f, 0.45f, 0.45f);
        }
    }
}
