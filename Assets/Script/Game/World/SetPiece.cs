using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

 

public class SetPiece
{ 
    public static Vector3Int Pos1;
    public static Vector3Int Pos2;
     
    public static void SaveSetPieceFile(Chunk setPiece, string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Resources/Set", fileName + ".json");
        File.WriteAllText(path, ToJson(setPiece));
    }

    public static Chunk LoadSetPieceFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Set/" + fileName);
        if (textAsset == null)
        {
            Debug.LogWarning("SetPiece not found: " + fileName);
            return null;
        }
        return FromJson(textAsset.text);
    }
    
    public static Chunk Copy()
    {
        Vector3Int min = new Vector3Int(Mathf.Min(Pos1.x, Pos2.x), Mathf.Min(Pos1.y, Pos2.y), Mathf.Min(Pos1.z, Pos2.z));
        Vector3Int max = new Vector3Int(Mathf.Max(Pos1.x, Pos2.x), Mathf.Max(Pos1.y, Pos2.y), Mathf.Max(Pos1.z, Pos2.z));
        Vector3Int extent = max - min + Vector3Int.one;
        Chunk setPiece = new Chunk(Mathf.Max(extent.x, Mathf.Max(extent.y, extent.z)));
        HashSet<Chunk> chunks = new HashSet<Chunk>();

        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    Vector3Int wp = new Vector3Int(x, y, z);
                    Chunk chunk = World.Inst[wp];
                    if (chunk == null || chunk == Chunk.Zero) continue;
                    chunks.Add(chunk);
                    Vector3Int lp = wp - min;
                    Vector3Int bp = World.GetBlockCoordinate(wp);
                    setPiece[lp.x, lp.y, lp.z] = chunk[bp.x, bp.y, bp.z];
                }

        foreach (Chunk chunk in chunks)
            foreach (Info entity in chunk.StaticEntity)
                if (IsEntityInRange(Vector3Int.FloorToInt(entity.position), min, max))
                    setPiece.StaticEntity.Add(new Info { id = entity.id, position = entity.position - min });

        return setPiece;
    }
    
    public static void Paste(Vector3Int position, Chunk setPiece, bool setCorners = false, bool authorMode = false)
        => Paste(World.Inst, position, setPiece, setCorners, authorMode);

    public static void Paste(World world, Vector3Int position, Chunk setPiece, bool setCorners = false, bool authorMode = false)
    {
        if (setPiece == null) return;
        if (setCorners)
        {
            Pos1 = position;
            Pos2 = position + Vector3Int.one * (setPiece.size - 1);
        }

        Vector3Int chunkPos, worldPos;
        int overlay = Block.ConvertID(ID.OverlayBlock);

        foreach (Info entity in setPiece.StaticEntity)
        {
            worldPos = position + Vector3Int.FloorToInt(entity.position);
            if (!IsInBounds(world, worldPos)) continue;
            chunkPos = World.GetChunkCoordinate(worldPos);
            Chunk chunk = world[chunkPos];
            if (chunk == null || chunk == Chunk.Zero) continue;
            Info info = Entity.CreateInfo(entity.id, worldPos);
            if (info == null) info = (Info)Helper.Clone(entity);
            if (info == null) continue;
            info.position = worldPos + SpawnOffsetOf(entity.id);
            chunk.StaticEntity.Add(info);
        }

        for (int x = 0; x < setPiece.size; x++)
        {
            for (int y = 0; y < setPiece.size; y++)
            {
                for (int z = 0; z < setPiece.size; z++)
                {
                    // 0 = fillable air (clear to air), -1/OverlayBlock = leave existing
                    // terrain as-is (unless authoring), positive = place that block.
                    int blockID = setPiece[x, y, z];
                    if (blockID == -1) blockID = overlay;
                    if (blockID == overlay && !authorMode) continue;
                    SetBlock(world, new Vector3Int(position.x + x, position.y + y, position.z + z), blockID);
                }
            }
        }
    }

    private static void SetBlock(World world, Vector3Int worldPos, int blockID)
    {
        if (!IsInBounds(world, worldPos)) return;
        Chunk chunk = world[worldPos];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk[World.GetBlockCoordinate(worldPos)] = blockID;
    }

    private static bool IsInBounds(World world, Vector3Int p)
    {
        return p.x >= 0 && p.x < world.Bounds.x &&
               p.y >= 0 && p.y < world.Bounds.y &&
               p.z >= 0 && p.z < world.Bounds.z;
    }

    /// <summary>The SpawnOffset for an entity id (centers its prefab in its cell), or zero.</summary>
    private static Vector3 SpawnOffsetOf(ID id)
        => Entity.Dictionary.TryGetValue(id, out Entity e) ? e.SpawnOffset : Vector3.zero;

    private static bool IsEntityInRange(Vector3Int coord, Vector3Int min, Vector3Int max)
    {
        return coord.x >= min.x && coord.x <= max.x &&
               coord.y >= min.y && coord.y <= max.y &&
               coord.z >= min.z && coord.z <= max.z;
    }

    // JSON format:
    //   { "size": N, "blocks": [N^3 ints, x-fastest then y then z], "entities": [{ "id": "PineTree", "x":0, "y":0, "z":0 }] }
    //   block value: 0 = fillable air (cleared to air), -1 = non-fillable air (left as-is), positive = block id.
    private static int Index(int size, int x, int y, int z) => x + size * (y + size * z);

    private static string ToJson(Chunk setPiece)
    {
        int size = setPiece.size;
        SetPieceData data = new SetPieceData
        {
            size = size,
            blocks = new int[size * size * size],
            entities = new SetPieceEntityData[setPiece.StaticEntity.Count]
        };
        int overlay = Block.ConvertID(ID.OverlayBlock);
        for (int z = 0; z < size; z++)
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int b = setPiece[x, y, z];
                    data.blocks[Index(size, x, y, z)] = b == overlay ? -1 : b;
                }
        for (int e = 0; e < setPiece.StaticEntity.Count; e++)
        {
            Info entity = setPiece.StaticEntity[e];
            Vector3Int p = Vector3Int.FloorToInt(entity.position);
            data.entities[e] = new SetPieceEntityData { id = entity.id.ToString(), x = p.x, y = p.y, z = p.z };
        }
        return JsonUtility.ToJson(data, true);
    }

    private static Chunk FromJson(string json)
    {
        SetPieceData data;
        try
        {
            data = JsonUtility.FromJson<SetPieceData>(json);
        }
        catch
        {
            return null;
        }
        if (data == null || data.size <= 0) return null;
        Chunk setPiece = new Chunk(data.size);
        int size = data.size;
        if (data.blocks != null)
        {
            for (int z = 0; z < size; z++)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int i = Index(size, x, y, z);
                        if (i < data.blocks.Length)
                        {
                            int b = data.blocks[i];
                            setPiece[x, y, z] = b == -1 ? Block.ConvertID(ID.OverlayBlock) : b;
                        }
                    }
        }
        if (data.entities != null)
        {
            foreach (SetPieceEntityData e in data.entities)
            {
                if (!Enum.TryParse(e.id, out ID stringID)) continue;
                setPiece.StaticEntity.Add(new Info { id = stringID, position = new Vector3(e.x, e.y, e.z) });
            }
        }
        return setPiece;
    }
}

[Serializable]
public class SetPieceData
{
    public int size;
    public int[] blocks;
    public SetPieceEntityData[] entities;
}

[Serializable]
public class SetPieceEntityData
{
    public string id;
    public int x;
    public int y;
    public int z;
}
