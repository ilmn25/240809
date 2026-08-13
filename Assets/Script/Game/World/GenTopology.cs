using System.Collections.Generic;
using UnityEngine;

public static class GenTopology
{
    public enum PlateKind { Main, Island, Void }

    public class Node
    {
        public float X, Z;
        public float Size = 1f;
        public bool IsSpawn;
        public int Ring;
        public PlateKind Kind;
        public BiomeType Biome;
        public readonly List<int> Links = new();
    }

    private const int RoomCount = 20;
    private const float BranchLength = 180f;
    private const float BranchWinding = 40f;
    private const float BranchTether = 0.36f;
    private const float IslandChance = 0.25f;
    private const float VoidChance = 0.20f;
    private const float IslandRadius = 0.42f;
    private const float CoastScale = 0.02f;
    private const int RimLinks = 3;
    private const float ConnectRadius = 130f;
    private const float EdgeFraction = 0.22f;
    private const float VoronoiNoise = 24f;
    private const float VoronoiScale = 0.045f;

    private static readonly BiomeType[] BiomePool =
    {
        BiomeType.Forest, BiomeType.Forest, BiomeType.Desert, BiomeType.Desert,
        BiomeType.Grass, BiomeType.Grass, BiomeType.Forest, BiomeType.Desert,
        BiomeType.Grass, BiomeType.Mountain, BiomeType.Forest, BiomeType.Mountain,
    };

    private static bool _generated;
    private static int _genSeed;
    private static Vector3Int _genBounds;
    private static float _coastOffset, _voronoiOffset, _branchWindingOffset;
    private static List<Node> _nodes;
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

        // DST-style circular coastline — beyond it is empty void.
        Vector3Int c = _nodeWorld[0];
        float radius = IslandRadius * Mathf.Min(World.Inst.Bounds.x, World.Inst.Bounds.z) * CoastlineNoise(x, z);
        if (Dist(x, z, c.x, c.z) > radius) return BiomeType.Void;

        if (!TryGetNearestTwo(x, z, out int best, out _)) return BiomeType.Grass;
        return _nodes[best].Kind == PlateKind.Void ? BiomeType.Void : _nodes[best].Biome;
    }

    private static float CoastlineNoise(int x, int z)
        => 0.92f + 0.16f * Mathf.PerlinNoise(x * CoastScale + _coastOffset, z * CoastScale + _coastOffset);

    private static void Pass1_BuildGraph(System.Random rng)
    {
        Vector3Int bounds = World.Inst.Bounds;
        float cx = bounds.x * 0.5f;
        float cz = bounds.z * 0.5f;
        float minDim = Mathf.Min(bounds.x, bounds.z);

        _nodes = new List<Node>();
        _nodes.Add(new Node { X = cx, Z = cz, Size = 1f, IsSpawn = true, Ring = 0, Kind = PlateKind.Main });

        float baseRadius = minDim * 0.13f;
        int placed = 1;
        int ring = 0;
        while (placed < RoomCount)
        {
            int perRing = ring == 0 ? 6 : (ring == 1 ? 8 : RoomCount - placed);
            float radius = baseRadius * (ring + 1);
            float angleOffset = (float)(rng.NextDouble() * Mathf.PI * 2);
            for (int k = 0; k < perRing && placed < RoomCount; k++)
            {
                float ang = angleOffset + (float)k / perRing * Mathf.PI * 2;
                ang += (float)(rng.NextDouble() - 0.5) * 0.35f; // organic wobble
                _nodes.Add(new Node
                {
                    X = cx + Mathf.Cos(ang) * radius,
                    Z = cz + Mathf.Sin(ang) * radius,
                    Size = 0.8f + (float)rng.NextDouble() * 0.8f,
                    IsSpawn = false,
                    Ring = ring + 1,
                    Kind = PlateKind.Main,
                });
                placed++;
            }
            ring++;
        }

        ClassifyPlates(rng);
        BuildMainIsland(rng);
        StretchBranches(cx, cz, minDim);
        AddRimLinks(rng, minDim);
    }

    private static void ClassifyPlates(System.Random rng)
    {
        _nodes[0].Kind = PlateKind.Main; // spawn hub
        for (int i = 1; i < _nodes.Count; i++)
        {
            if (_nodes[i].Ring <= 1) { _nodes[i].Kind = PlateKind.Main; continue; }
            double roll = rng.NextDouble();
            if (roll < VoidChance) _nodes[i].Kind = PlateKind.Void;
            else if (roll < VoidChance + IslandChance) _nodes[i].Kind = PlateKind.Island;
            else _nodes[i].Kind = PlateKind.Main;
        }
    }

    private static void BuildMainIsland(System.Random rng)
    {
        var main = new List<int>();
        for (int i = 0; i < _nodes.Count; i++)
            if (_nodes[i].Kind == PlateKind.Main) main.Add(i);
        if (main.Count == 0) return;

        var inMst = new HashSet<int>();
        var minDist = new Dictionary<int, float>();
        var minEdge = new Dictionary<int, int>();
        foreach (int i in main) { minDist[i] = float.MaxValue; minEdge[i] = -1; }
        minDist[main[0]] = 0;

        for (int step = 0; step < main.Count; step++)
        {
            int u = -1; float best = float.MaxValue;
            foreach (int i in main)
                if (!inMst.Contains(i) && minDist[i] < best) { best = minDist[i]; u = i; }
            if (u < 0) break;
            inMst.Add(u);
            if (minEdge[u] >= 0)
            {
                _nodes[u].Links.Add(minEdge[u]);
                _nodes[minEdge[u]].Links.Add(u);
            }
            foreach (int v in main)
            {
                if (inMst.Contains(v)) continue;
                float d = Dist(_nodes[u].X, _nodes[u].Z, _nodes[v].X, _nodes[v].Z);
                if (d <= ConnectRadius && d < minDist[v]) { minDist[v] = d; minEdge[v] = u; }
            }
        }
    }

    private static void StretchBranches(float cx, float cz, float minDim)
    {
        float branchTether = BranchTether * minDim;
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].IsSpawn || _nodes[i].Kind != PlateKind.Main || _nodes[i].Links.Count != 1) continue;
            int parent = _nodes[i].Links[0];
            float dx = _nodes[i].X - cx;
            float dz = _nodes[i].Z - cz;
            float d = Mathf.Sqrt(dx * dx + dz * dz);
            if (d < 0.01f) continue;
            float nx = dx / d, nz = dz / d;

            float wob = (Mathf.PerlinNoise(
                _nodes[i].X * 0.02f + _branchWindingOffset,
                _nodes[i].Z * 0.02f + _branchWindingOffset) - 0.5f) * 2f * BranchWinding;

            float length = BranchLength * _nodes[i].Size;
            _nodes[i].X = _nodes[parent].X + nx * length - nz * wob;
            _nodes[i].Z = _nodes[parent].Z + nz * length + nx * wob;

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
    }

    private static void AddRimLinks(System.Random rng, float minDim)
    {
        float edgeRadius = EdgeFraction * minDim;
        var edgeMains = new List<int>();
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].Kind != PlateKind.Main) continue;
            if (Dist(_nodes[i].X, _nodes[i].Z, _nodes[0].X, _nodes[0].Z) >= edgeRadius)
                edgeMains.Add(i);
        }
        if (edgeMains.Count < 2) return;

        int added = 0, attempts = 0;
        while (added < RimLinks && attempts++ < RimLinks * 20)
        {
            int a = edgeMains[rng.Next(edgeMains.Count)];
            int b = -1; float best = float.MaxValue;
            foreach (int c in edgeMains)
            {
                if (c == a || _nodes[a].Links.Contains(c)) continue;
                float d = Dist(_nodes[a].X, _nodes[a].Z, _nodes[c].X, _nodes[c].Z);
                if (d < best) { best = d; b = c; }
            }
            if (b < 0 || _nodes[a].Links.Contains(b)) continue;
            _nodes[a].Links.Add(b);
            _nodes[b].Links.Add(a);
            added++;
        }
    }

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

    private static void Pass3_PrepareWorld()
    {
        _nodeWorld = new Vector3Int[_nodes.Count];
        for (int i = 0; i < _nodes.Count; i++)
            _nodeWorld[i] = new Vector3Int((int)_nodes[i].X, 0, (int)_nodes[i].Z);
    }

    public static Vector3Int[] GetMountainCenters()
    {
        EnsureGenerated();
        int count = 0;
        for (int i = 0; i < _nodeWorld.Length; i++)
            if (_nodes[i].Biome == BiomeType.Mountain) count++;
        var centers = new Vector3Int[count];
        int c = 0;
        for (int i = 0; i < _nodeWorld.Length; i++)
            if (_nodes[i].Biome == BiomeType.Mountain) centers[c++] = _nodeWorld[i];
        return centers;
    }

    public static bool IsLandConnection(int x, int z)
    {
        EnsureGenerated();
        if (!TryGetNearestTwo(x, z, out int a, out int b)) return false;
        if (_nodes[a].Kind == PlateKind.Void || _nodes[b].Kind == PlateKind.Void) return false;
        return _nodes[a].Links.Contains(b);
    }

    public static bool TryGetBiomeBoundaryGap(int x, int z, out float gap)
    {
        EnsureGenerated();
        gap = float.MaxValue;
        if (_nodeWorld == null || _nodeWorld.Length < 2) return false;

        if (!TryGetNearestTwo(x, z, out int bestA, out int bestB)) return false;
        if (_nodes[bestA].Kind == PlateKind.Void || _nodes[bestB].Kind == PlateKind.Void) return false;

        float dA = JitteredDistance(x, z, bestA);
        float dB = JitteredDistance(x, z, bestB);
        gap = dB - dA;
        return true;
    }

    /// <summary>
    /// True when (x,z) sits on the boundary between a land plate and a void
    /// plate, within <paramref name="maxGap"/> blocks of the seam. Used to build
    /// barrier mountains along the coastline instead of leaving empty void.
    /// </summary>
    public static bool TryGetVoidBoundaryGap(int x, int z, out float gap)
    {
        EnsureGenerated();
        gap = float.MaxValue;
        if (_nodeWorld == null || _nodeWorld.Length < 2) return false;

        if (!TryGetNearestTwo(x, z, out int bestA, out int bestB)) return false;
        bool aVoid = _nodes[bestA].Kind == PlateKind.Void;
        bool bVoid = _nodes[bestB].Kind == PlateKind.Void;
        if (aVoid == bVoid) return false; // both land or both void — not a coastline

        float dA = JitteredDistance(x, z, bestA);
        float dB = JitteredDistance(x, z, bestB);
        gap = dB - dA;
        return true;
    }

    private static bool TryGetNearestTwo(int x, int z, out int a, out int b)
    {
        a = -1; b = -1;
        if (_nodeWorld == null || _nodeWorld.Length < 2) return false;

        float dA = float.MaxValue, dB = float.MaxValue;
        for (int i = 0; i < _nodeWorld.Length; i++)
        {
            float d = JitteredDistance(x, z, i);
            if (d < dA) { dB = dA; b = a; dA = d; a = i; }
            else if (d < dB) { dB = d; b = i; }
        }
        return a >= 0 && b >= 0;
    }

    private static float JitteredDistance(int x, int z, int nodeIdx)
    {
        float jitter = (Mathf.PerlinNoise(
            x * VoronoiScale + _voronoiOffset + nodeIdx * 0.37f,
            z * VoronoiScale + _voronoiOffset) - 0.5f) * 2f * VoronoiNoise;
        return Dist(x, z, _nodeWorld[nodeIdx].x, _nodeWorld[nodeIdx].z) + jitter;
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
