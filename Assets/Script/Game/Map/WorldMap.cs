using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>A single structure marker on the map (a static entity's world column).</summary>
[Serializable]
public class MapMarker
{
    public ID id;
    public int x, z;
}

/// <summary>
/// Per-world 2D map state (Don't Starve style). Stores which columns have been
/// explored (fog of war), caches structure markers, and generates a top-down
/// terrain texture. Persisted with the World via BinaryFormatter, so the
/// runtime-only fields (Texture, Dirty) are marked [NonSerialized].
/// </summary>
[Serializable]
public class WorldMap
{
    /// <summary>Per-column explored flag, indexed z * width + x. 0 = fog, 1 = revealed.</summary>
    public byte[] Explored;
    /// <summary>Cached structure markers (built once per world).</summary>
    public List<MapMarker> Markers = new List<MapMarker>();

    [NonSerialized] public Texture2D Texture;
    [NonSerialized] public bool Dirty = true;
    [NonSerialized] private bool _markersBuilt;

    private static readonly Color FogColor = new Color(0.08f, 0.08f, 0.10f, 1f);
    private static readonly Color VoidColor = new Color(0.15f, 0.25f, 0.45f, 1f);

    /// <summary>Marks all columns within <paramref name="radius"/> of (x, z) as explored.</summary>
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

    /// <summary>Scans every chunk's static entities once and caches their map markers.</summary>
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
                    {
                        if (info == null) continue;
                        Markers.Add(new MapMarker
                        {
                            id = info.id,
                            x = (int)info.position.x,
                            z = (int)info.position.z
                        });
                    }
                }
            }
        }
        _markersBuilt = true;
    }

    /// <summary>Rebuilds the map texture from explored state, terrain, and markers.</summary>
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

        // Terrain + fog
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
                int block = 0;
                for (int cy = world.Size.y - 1; cy >= 0 && block == 0; cy--)
                {
                    Chunk chunk = world[cx * World.ChunkSize, cy * World.ChunkSize, cz * World.ChunkSize];
                    if (chunk == null) continue;
                    for (int ly = World.ChunkSize - 1; ly >= 0; ly--)
                    {
                        block = chunk[lx, ly, lz];
                        if (block != 0) break;
                    }
                }
                pixels[idx] = block != 0 ? GetBlockColor(block) : VoidColor;
            }
        }

        // Structure markers (only in explored columns)
        foreach (MapMarker m in Markers)
        {
            if (m.x < 0 || m.x >= width || m.z < 0 || m.z >= height) continue;
            int idx = m.z * width + m.x;
            if (Explored[idx] != 0)
                pixels[idx] = GetMarkerColor(m.id);
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
            case ID.GrassBlock: return new Color(0.55f, 0.42f, 0.28f);
            case ID.ForestBlock: return new Color(0.20f, 0.50f, 0.20f);
            case ID.SandBlock: return new Color(0.85f, 0.80f, 0.50f);
            case ID.StoneBlock: return new Color(0.50f, 0.50f, 0.50f);
            case ID.GraniteBlock: return new Color(0.62f, 0.52f, 0.52f);
            case ID.MarbleBlock: return new Color(0.85f, 0.85f, 0.85f);
            case ID.BrickBlock: return new Color(0.60f, 0.30f, 0.20f);
            case ID.WoodBlock: return new Color(0.50f, 0.35f, 0.20f);
            case ID.BackroomBlock: return new Color(0.80f, 0.80f, 0.20f);
            case ID.MulchBlock: return new Color(0.40f, 0.30f, 0.20f);
            default: return new Color(0.40f, 0.40f, 0.40f);
        }
    }

    private static Color GetMarkerColor(ID id)
    {
        switch (id)
        {
            case ID.PineTree:
            case ID.BirchTree: return new Color(0.10f, 0.60f, 0.10f);
            case ID.Chest: return new Color(0.90f, 0.70f, 0.20f);
            case ID.Bush: return new Color(0.30f, 0.70f, 0.30f);
            case ID.Grass: return new Color(0.40f, 0.80f, 0.40f);
            case ID.Deathcap: return new Color(0.80f, 0.20f, 0.20f);
            case ID.Orchids: return new Color(0.90f, 0.50f, 0.90f);
            default: return new Color(1f, 1f, 1f);
        }
    }
}
