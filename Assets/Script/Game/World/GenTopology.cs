using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A branch-and-loop world topology generator.
///
/// Procedural passes:
///   Pass 1 — BuildGraph: place biome "room" nodes with a physics repulsion
///            simulation so they spread organically with a natural minimum
///            spacing, tethered inside the map; then link them into a minimum
///            spanning tree whose leaf nodes are the dead-end "branches", and
///            finally close "loops" between nearby rooms so the map has rings.
///   Pass 2 — AssignBiomes: each node is assigned a biome task. Extend BiomePool
///            to add more biomes later.
///   Pass 3 — PrepareWorld: bake node world positions and deduplicated links.
///
/// Land is the Voronoi diagram of the room nodes (biome islands), with narrow
/// land bridges along the branch/loop network connecting the islands together,
/// and ocean filling the gaps between branches.
/// </summary>
public static class GenTopology
{
    public class Node
    {
        public float X, Z;               // world-space centre
        public float Size = 1f;          // island/branch scale — varied for an organic look
        public bool IsSpawn;
        public BiomeType Biome;
        public readonly List<int> Links = new();
    }

    // --- Tuning -------------------------------------------------------------
    /// <summary>Number of biome "room" nodes placed by the repulsion sim.</summary>
    private const int RoomCount = 20;
    /// <summary>Base spacing between room nodes (blocks) — scaled by each pair's
    /// size so nodes cluster organically instead of forming a uniform lattice.</summary>
    private const float MinSpacing = 32f;
    /// <summary>Rooms are tethered within this fraction of the map size from centre.</summary>
    private const float TetherRadius = 0.28f;
    /// <summary>How far a dead-end branch is stretched outward from its parent.</summary>
    private const float BranchLength = 180f;
    /// <summary>How much a branch winds sideways (blocks) so it isn't a straight line.</summary>
    private const float BranchWinding = 40f;
    /// <summary>Stretched branch tips are tethered within this fraction of the map size.</summary>
    private const float BranchTether = 0.44f;
    /// <summary>Half-width (blocks) of the land bridge connecting biome islands.</summary>
    private const float BridgeWidth = 12f;
    /// <summary>Land extends this far around a room centre (blocks) — the island.
    /// Kept smaller than node spacing so islands stay distinct, joined only by bridges.</summary>
    private const float LandRadius = 48f;
    /// <summary>Coastline wobble in blocks — makes the land edge organic.</summary>
    private const float LandNoise = 20f;
    private const float CoastScale = 0.02f;
    /// <summary>Biome border wobble in blocks — organic voronoi seams.</summary>
    private const float VoronoiNoise = 24f;
    private const float VoronoiScale = 0.045f;
    /// <summary>How often to close a circuit between nearby rooms. 0 = never
    /// (pure tree), ~0.35 = default, higher = always.</summary>
    private const double LoopChance = 0.35;
    /// <summary>Max distance (blocks) for a loop link between unlinked rooms.</summary>
    private const float LoopLinkRadius = 110f;

    /// <summary>Biome tasks handed out to branch nodes (extend this to add biomes).</summary>
    private static readonly BiomeType[] BiomePool =
    {
        BiomeType.Forest, BiomeType.Forest, BiomeType.Desert, BiomeType.Desert,
        BiomeType.Grass, BiomeType.Grass, BiomeType.Forest, BiomeType.Desert,
        BiomeType.Grass, BiomeType.Forest, BiomeType.Desert, BiomeType.Grass,
    };

    // --- Pass results -------------------------------------------------------
    private static bool _generated;
    private static int _genSeed;
    private static Vector3Int _genBounds;
    private static float _coastOffset, _voronoiOffset, _branchWindingOffset;
    private static List<Node> _nodes;
    private static List<Vector2Int> _links;
    private static Vector3Int[] _nodeWorld;

    public static void EnsureGenerated()
    {
        if (World.Inst == null) return;
        Vector3Int bounds = World.Inst.Bounds;
        if (_generated && _genSeed == Save.Inst.seed && _genBounds == bounds) return;

        _generated = true;
        _genSeed = Save.Inst.seed;
        _genBounds = bounds;
        _coastOffset = Gen.GetDeterministicOffset("TopologyCoast");
        _voronoiOffset = Gen.GetDeterministicOffset("TopologyVoronoi");
        _branchWindingOffset = Gen.GetDeterministicOffset("TopologyBranch");

        System.Random rng = Gen.CreateChunkRandom("Topology", Vector3Int.zero);
        Pass1_BuildGraph(rng);
        Pass2_AssignBiomes(rng);
        Pass3_PrepareWorld();
    }

    public static BiomeType GetBiome(int x, int z)
    {
        EnsureGenerated();
        if (_nodes == null || _nodes.Count == 0) return BiomeType.Grass;

        // Voronoi island: the nearest room node decides the biome.
        int best = -1;
        float bestD = float.MaxValue;
        for (int i = 0; i < _nodeWorld.Length; i++)
        {
            float d = JitteredDistance(x, z, i);
            if (d < bestD) { bestD = d; best = i; }
        }

        // Land = the island blob around a room, OR a land bridge along the
        // branch/loop network, so the biome islands stay connected. Island size
        // varies per room — big biomes are large, small ones are pockets.
        float coast = LandNoise * (Mathf.PerlinNoise(x * CoastScale + _coastOffset, z * CoastScale + _coastOffset) - 0.5f);
        float islandRadius = LandRadius * _nodes[best].Size;
        if (bestD > islandRadius + coast && DistanceToNetwork(x, z) > BridgeWidth)
            return BiomeType.Ocean;

        return _nodes[best].Biome;
    }

    /// <summary>Pass 1 — place room nodes with a repulsion simulation, then
    /// connect them into a branch-and-loop graph.</summary>
    private static void Pass1_BuildGraph(System.Random rng)
    {
        Vector3Int bounds = World.Inst.Bounds;
        float cx = bounds.x * 0.5f;
        float cz = bounds.z * 0.5f;
        float spawnRadius = bounds.x * 0.30f;

        // Seed rooms across the central disc; the spawn sits exactly at centre.
        _nodes = new List<Node>();
        for (int i = 0; i < RoomCount; i++)
        {
            float ang = (float)(rng.NextDouble() * Mathf.PI * 2);
            float rad = spawnRadius * Mathf.Sqrt((float)rng.NextDouble()); // uniform in disc
            _nodes.Add(new Node
            {
                X = cx + Mathf.Cos(ang) * rad,
                Z = cz + Mathf.Sin(ang) * rad,
                Size = 0.7f + (float)rng.NextDouble() * 0.9f, // 0.7..1.6 — big & small rooms
                IsSpawn = i == 0,
            });
        }
        _nodes[0].X = cx;
        _nodes[0].Z = cz;

        // Repulsion / collision resolution: push rooms apart to a natural minimum
        // spacing while tethering them inside the map.
        float tether = TetherRadius * Mathf.Min(bounds.x, bounds.z);
        for (int iter = 0; iter < 140; iter++)
        {
            for (int a = 0; a < _nodes.Count; a++)
            {
                for (int b = a + 1; b < _nodes.Count; b++)
                {
                    float dx = _nodes[b].X - _nodes[a].X;
                    float dz = _nodes[b].Z - _nodes[a].Z;
                    float d = Mathf.Sqrt(dx * dx + dz * dz);
                    // Bigger rooms repel further, so big islands space out and
                    // small rooms cluster together — no uniform lattice.
                    float spacing = MinSpacing * (_nodes[a].Size + _nodes[b].Size) * 0.5f;
                    if (d < spacing && d > 0.001f)
                    {
                        float push = (spacing - d) * 0.5f;
                        float nx = dx / d, nz = dz / d;
                        _nodes[a].X -= nx * push; _nodes[a].Z -= nz * push;
                        _nodes[b].X += nx * push; _nodes[b].Z += nz * push;
                    }
                }
            }
            for (int i = 0; i < _nodes.Count; i++)
            {
                float dx = _nodes[i].X - cx;
                float dz = _nodes[i].Z - cz;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d > tether)
                {
                    float s = tether / d;
                    _nodes[i].X = cx + dx * s;
                    _nodes[i].Z = cz + dz * s;
                }
            }
        }

        // Connectivity — build a minimum spanning tree over the room cloud:
        // every room is reachable, and the tree's leaf nodes form the dead-end
        // "branches" that stretch out from the main path into the map.
        bool[] inMst = new bool[_nodes.Count];
        float[] minDist = new float[_nodes.Count];
        int[] minEdge = new int[_nodes.Count];
        for (int i = 0; i < _nodes.Count; i++) { minDist[i] = float.MaxValue; minEdge[i] = -1; }
        minDist[0] = 0;

        for (int step = 0; step < _nodes.Count; step++)
        {
            int u = -1;
            float best = float.MaxValue;
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (!inMst[i] && minDist[i] < best) { best = minDist[i]; u = i; }
            }
            if (u < 0) break;
            inMst[u] = true;
            if (minEdge[u] >= 0)
            {
                _nodes[u].Links.Add(minEdge[u]);
                _nodes[minEdge[u]].Links.Add(u);
            }
            for (int v = 0; v < _nodes.Count; v++)
            {
                if (inMst[v]) continue;
                float d = Dist(_nodes[u].X, _nodes[u].Z, _nodes[v].X, _nodes[v].Z);
                if (d < minDist[v]) { minDist[v] = d; minEdge[v] = u; }
            }
        }

        // Loops: close circuits between nearby rooms so the map has rings
        // instead of a pure tree — you can walk in a full circle instead of
        // always hitting a dead-end.
        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                if (_nodes[i].Links.Contains(j)) continue;
                if (rng.NextDouble() > LoopChance) continue;
                if (Dist(_nodes[i].X, _nodes[i].Z, _nodes[j].X, _nodes[j].Z) < LoopLinkRadius)
                {
                    _nodes[i].Links.Add(j);
                    _nodes[j].Links.Add(i);
                }
            }
        }

        // Branching: stretch every dead-end leaf room outward from its parent
        // so branches become long, winding peninsulas.
        // The land bridge along each link connects the branch island back to
        // the mainland, so branches read as walkable dead-end fingers.
        float branchTether = BranchTether * Mathf.Min(bounds.x, bounds.z);
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].IsSpawn || _nodes[i].Links.Count != 1) continue;
            int parent = _nodes[i].Links[0];
            float dx = _nodes[i].X - _nodes[parent].X;
            float dz = _nodes[i].Z - _nodes[parent].Z;
            float d = Mathf.Sqrt(dx * dx + dz * dz);
            if (d < 0.01f) continue;
            float nx = dx / d, nz = dz / d;

            // Slight sideways wobble so the branch isn't a straight line.
            float wob = (Mathf.PerlinNoise(
                _nodes[i].X * 0.02f + _branchWindingOffset,
                _nodes[i].Z * 0.02f + _branchWindingOffset) - 0.5f) * 2f * BranchWinding;

            float length = BranchLength * _nodes[i].Size;
            _nodes[i].X = _nodes[parent].X + nx * length - nz * wob;
            _nodes[i].Z = _nodes[parent].Z + nz * length + nx * wob;

            // Keep the branch tip inside the map.
            float ox = _nodes[i].X - cx;
            float oz = _nodes[i].Z - cz;
            float od = Mathf.Sqrt(ox * ox + oz * oz);
            if (od > branchTether)
            {
                float s = branchTether / od;
                _nodes[i].X = cx + ox * s;
                _nodes[i].Z = cz + oz * s;
            }
        }

        // Deduplicate undirected links into a flat list.
        _links = new List<Vector2Int>();
        var seen = new HashSet<long>();
        for (int i = 0; i < _nodes.Count; i++)
        {
            foreach (int j in _nodes[i].Links)
            {
                int a = Mathf.Min(i, j), b = Mathf.Max(i, j);
                if (a == b) continue;
                long key = (long)a * 100000 + b;
                if (seen.Add(key)) _links.Add(new Vector2Int(a, b));
            }
        }
    }

    /// <summary>Pass 2 — assign a biome task to every node (spawn is grassland).</summary>
    private static void Pass2_AssignBiomes(System.Random rng)
    {
        _nodes[0].Biome = BiomeType.Grass;

        var pool = new List<BiomeType>(BiomePool);
        Shuffle(pool, rng);

        int p = 0;
        for (int i = 1; i < _nodes.Count; i++)
        {
            BiomeType assigned = pool[p % pool.Count]; p++;
            // Prefer a biome different from its neighbours (task adjacency).
            for (int tries = 0; tries < 3; tries++)
            {
                bool clash = false;
                foreach (int nb in _nodes[i].Links)
                    if (_nodes[nb].Biome == assigned) { clash = true; break; }
                if (!clash) break;
                assigned = pool[p % pool.Count]; p++;
            }
            _nodes[i].Biome = assigned;
        }
    }

    /// <summary>Pass 3 — bake node world positions.</summary>
    private static void Pass3_PrepareWorld()
    {
        _nodeWorld = new Vector3Int[_nodes.Count];
        for (int i = 0; i < _nodes.Count; i++)
            _nodeWorld[i] = new Vector3Int((int)_nodes[i].X, 0, (int)_nodes[i].Z);
    }

    /// <summary>Distance from (x,z) to the nearest link or room centre — the
    /// branch/loop network that the land bridges are built along.</summary>
    private static float DistanceToNetwork(int x, int z)
    {
        float min = float.MaxValue;
        for (int i = 0; i < _links.Count; i++)
        {
            Vector3Int a = _nodeWorld[_links[i].x];
            Vector3Int b = _nodeWorld[_links[i].y];
            min = Mathf.Min(min, DistPointSegment(x, z, a.x, a.z, b.x, b.z));
        }
        for (int i = 0; i < _nodeWorld.Length; i++)
            min = Mathf.Min(min, DistPointPoint(x, z, _nodeWorld[i].x, _nodeWorld[i].z));
        return min;
    }

    /// <summary>True if (x,z) lies on a land bridge — the narrow connections
    /// along the branch/loop network that keep the biome islands connected.
    /// Ravines must not cut these.</summary>
    public static bool IsBridge(int x, int z)
    {
        EnsureGenerated();
        return _links != null && _links.Count > 0 && DistanceToNetwork(x, z) <= BridgeWidth;
    }

    /// <summary>
    /// True when (x,z) sits on the voronoi ridge between two nodes that have
    /// different biomes, within <paramref name="maxGap"/> blocks of the ridge.
    /// Ravines carve along these ridges to separate biomes.
    /// </summary>
    public static bool IsBiomeBoundary(int x, int z, float maxGap)
    {
        EnsureGenerated();
        if (_nodeWorld == null || _nodeWorld.Length < 2) return false;

        int bestA = -1, bestB = -1;
        float dA = float.MaxValue, dB = float.MaxValue;
        for (int i = 0; i < _nodeWorld.Length; i++)
        {
            float d = JitteredDistance(x, z, i);
            if (d < dA) { dB = dA; bestB = bestA; dA = d; bestA = i; }
            else if (d < dB) { dB = d; bestB = i; }
        }
        if (bestA < 0 || bestB < 0) return false;
        if (_nodes[bestA].Biome == _nodes[bestB].Biome) return false;
        return (dB - dA) < maxGap;
    }

    /// <summary>Per-node jittered distance used for both biome assignment and
    /// boundary detection, so ravines line up exactly with biome borders.</summary>
    private static float JitteredDistance(int x, int z, int nodeIdx)
    {
        float jitter = (Mathf.PerlinNoise(
            x * VoronoiScale + _voronoiOffset + nodeIdx * 0.37f,
            z * VoronoiScale + _voronoiOffset) - 0.5f) * 2f * VoronoiNoise;
        return DistPointPoint(x, z, _nodeWorld[nodeIdx].x, _nodeWorld[nodeIdx].z) + jitter;
    }

    private static float DistPointPoint(int x, int z, int px, int pz)
        => Mathf.Sqrt((x - px) * (x - px) + (z - pz) * (z - pz));

    private static float DistPointSegment(int x, int z, int ax, int az, int bx, int bz)
    {
        float vx = bx - ax, vz = bz - az;
        float len2 = vx * vx + vz * vz;
        float t = len2 == 0 ? 0 : Mathf.Clamp01(((x - ax) * vx + (z - az) * vz) / len2);
        float px = ax + vx * t, pz = az + vz * t;
        return Mathf.Sqrt((x - px) * (x - px) + (z - pz) * (z - pz));
    }

    private static float Dist(float ax, float az, float bx, float bz)
        => Mathf.Sqrt((ax - bx) * (ax - bx) + (az - bz) * (az - bz));

    private static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
