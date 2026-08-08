using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>A single structure marker on the map (a static entity's world column).</summary>
[Serializable]
public class MapMarker
{
    public ID id;
    public int x, z;
    public int y;
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

    /// <summary>How far (in blocks) above or below the surface a structure must be
    /// to appear on the map. Deeper structures are hidden.</summary>
    private const int SurfaceBand = 8;

    /// <summary>Scans every chunk's static entities and caches their map markers.
    /// Only structures near the surface are kept, so underground content stays hidden.
    /// Must be called right after world generation, while all static entities are
    /// still stored in the chunk lists (before any are loaded into the world).</summary>
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

    /// <summary>Adds a marker for a static entity if it's near the surface.</summary>
    private void AddMarker(World world, Info info)
    {
        if (info == null) return;
        int x = (int)info.position.x;
        int z = (int)info.position.z;
        int y = (int)info.position.y;

        // Only keep structures near the surface.
        int surfaceY = FindSurfaceY(world, x, z);
        if (surfaceY < 0 || Mathf.Abs(y - surfaceY) > SurfaceBand) return;

        Markers.Add(new MapMarker { id = info.id, x = x, z = z, y = y });
    }

    /// <summary>Scans a column top-to-bottom for the first air block above a solid
    /// block — the ground surface. Returns -1 if no surface exists.</summary>
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

    /// <summary>Rebuilds the map texture from explored state, terrain, and markers.
    /// Unexplored columns render as fog.</summary>
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
                int surfaceY = 0;
                for (int cy = world.Size.y - 1; cy >= 0 && block == 0; cy--)
                {
                    Chunk chunk = world[cx * World.ChunkSize, cy * World.ChunkSize, cz * World.ChunkSize];
                    if (chunk == null) continue;
                    for (int ly = World.ChunkSize - 1; ly >= 0; ly--)
                    {
                        block = chunk[lx, ly, lz];
                        if (block != 0)
                        {
                            surfaceY = cy * World.ChunkSize + ly;
                            break;
                        }
                    }
                }

                Color color = block != 0 ? GetBlockColor(block) : VoidColor;
                // Elevation → brightness: higher ground is brighter, lower is darker.
                color *= GetElevationBrightness(surfaceY, world.Bounds.y);
                pixels[idx] = color;
            }
        }

        // Black cover at the explored edge: darken explored columns near the fog
        // boundary with a gradient, so the edge reads as a visible black fade.
        const int EdgeWidth = 4;
        for (int wz = 0; wz < height; wz++)
        {
            for (int wx = 0; wx < width; wx++)
            {
                int idx = wz * width + wx;
                if (Explored[idx] == 0) continue;

                // Distance to the nearest unexplored column (0 = adjacent).
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
                    // Closer to fog = darker; fade out over the edge width.
                    float t = 1f - (dist / (float)EdgeWidth);
                    float darken = Mathf.Lerp(1f, 0.05f, t);
                    pixels[idx] *= darken;
                }
            }
        }

        Texture.SetPixels(pixels);
        Texture.Apply();
        Dirty = false;
    }

    /// <summary>Maps a surface height to a brightness multiplier (0.5–1.5) so
    /// elevation is visible on the map. Higher ground is brighter.</summary>
    private static float GetElevationBrightness(int surfaceY, int worldHeight)
    {
        if (worldHeight <= 0) return 1f;
        float t = Mathf.Clamp01((float)surfaceY / worldHeight);
        return Mathf.Lerp(0.5f, 1.5f, t);
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
}
