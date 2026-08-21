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
        int minX = Mathf.Min(Pos1.x, Pos2.x);
        int minY = Mathf.Min(Pos1.y, Pos2.y);
        int minZ = Mathf.Min(Pos1.z, Pos2.z);
        int maxX = Mathf.Max(Pos1.x, Pos2.x);
        int maxY = Mathf.Max(Pos1.y, Pos2.y);
        int maxZ = Mathf.Max(Pos1.z, Pos2.z);

        Chunk setPiece = new Chunk(Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ) + 1);
        Vector3Int min = new Vector3Int(minX, minY, minZ);
        Vector3Int chunkPos, worldPos, localPos, blockPos;
        List<Vector3Int> scannedChunks = new List<Vector3Int>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    worldPos = new Vector3Int(x, y, z);
                    localPos = worldPos - min;
                    chunkPos = World.GetChunkCoordinate(worldPos); 
                    blockPos = worldPos - chunkPos; 

                    Chunk chunk = World.Inst[chunkPos.x, chunkPos.y, chunkPos.z];
                    setPiece[localPos.x, localPos.y, localPos.z] = chunk[blockPos.x, blockPos.y, blockPos.z];
                    
                    if (!scannedChunks.Contains(chunkPos))
                    {
                        scannedChunks.Add(chunkPos);
                    } 
                }
            }
        }

        foreach (Vector3Int chunkCoord in scannedChunks)
        {
            Chunk chunk = World.Inst[chunkCoord.x, chunkCoord.y, chunkCoord.z];
            
            foreach (Info entity in chunk.StaticEntity)
            {
                if (IsEntityInRange(Vector3Int.FloorToInt(entity.position), minX, minY, minZ, maxX, maxY, maxZ))
                {
                    setPiece.StaticEntity.Add(new Info
                    {
                        id = entity.id,
                        position = entity.position - new Vector3Int(minX, minY, minZ)
                    });
                }
            }
        }

        return setPiece;
    }
    
    public static void Paste(Vector3Int position, Chunk setPiece, bool setCorners = false)
        => Paste(World.Inst, position, setPiece, setCorners);

    public static void Paste(World world, Vector3Int position, Chunk setPiece, bool setCorners = false)
    {
        if (setPiece == null) return;
        if (setCorners)
        {
            Pos1 = position;
            Pos2 = position + Vector3Int.one * (setPiece.size - 1);
        }

        Vector3Int chunkPos, worldPos;

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
            info.position = worldPos;
            chunk.StaticEntity.Add(info);
        }

        for (int x = 0; x < setPiece.size; x++)
        {
            for (int y = 0; y < setPiece.size; y++)
            {
                for (int z = 0; z < setPiece.size; z++)
                {
                    // 0 = fillable air (clear to air), -1 = non-fillable (leave existing
                    // terrain as-is), positive = place that block.
                    int blockID = setPiece[x, y, z];
                    if (blockID == -1) continue;
                    worldPos = new Vector3Int(position.x + x, position.y + y, position.z + z);
                    if (!IsInBounds(world, worldPos)) continue;
                    chunkPos = World.GetChunkCoordinate(worldPos);
                    Chunk chunk = world[chunkPos];
                    if (chunk == null || chunk == Chunk.Zero) continue;
                    chunk[World.GetBlockCoordinate(worldPos)] = blockID;
                }
            }
        }
    }

    private static bool IsInBounds(World world, Vector3Int p)
    {
        return p.x >= 0 && p.x < world.Bounds.x &&
               p.y >= 0 && p.y < world.Bounds.y &&
               p.z >= 0 && p.z < world.Bounds.z;
    }


    private static bool IsEntityInRange(Vector3Int coord, int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        return coord.x >= minX && coord.x <= maxX &&
               coord.y >= minY && coord.y <= maxY &&
               coord.z >= minZ && coord.z <= maxZ;
    }

    // JSON format:
    //   { "size": N, "blocks": [N^3 ints, x-fastest then y then z], "entities": [{ "id": "PineTree", "x":0, "y":0, "z":0 }] }
    //   block value: 0 = fillable air (cleared to air), -1 = non-fillable air (left as-is), positive = block id.
    private static string ToJson(Chunk setPiece)
    {
        int size = setPiece.size;
        SetPieceData data = new SetPieceData
        {
            size = size,
            blocks = new int[size * size * size],
            entities = new SetPieceEntityData[setPiece.StaticEntity.Count]
        };
        for (int z = 0; z < size; z++)
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    data.blocks[x + size * (y + size * z)] = setPiece[x, y, z];
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
                        int i = x + size * (y + size * z);
                        if (i < data.blocks.Length)
                            setPiece[x, y, z] = data.blocks[i];
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
