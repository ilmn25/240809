using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.Serialization;
 

public class SetPiece
{ 
    public static Vector3Int Pos1;
    public static Vector3Int Pos2;
     
    public static void SaveSetPieceFile(Chunk setPiece, string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Resources/Set", fileName + ".bytes");

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            Helper.BinaryFormatter.Serialize(stream, setPiece);
        }
    }

    public static Chunk LoadSetPieceFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Set/" + fileName);
        using (MemoryStream stream = new MemoryStream(textAsset.bytes))
        { 
            return (Chunk)Helper.BinaryFormatter.Deserialize(stream);
        }
    }
    
    public static Chunk Copy()
    { 
        Info info;
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
            
            // Check and add static entities
            foreach (Info entity in chunk.StaticEntity)
            {
                if (IsEntityInRange(Vector3Int.FloorToInt(entity.position) , minX, minY, minZ, maxX, maxY, maxZ))
                {
                    info = (Info)Helper.Clone(entity);
                    info.position = entity.position - new Vector3Int(minX, minY, minZ);
                    setPiece.StaticEntity.Add(info);
                }
            }

            // Check and add dynamic entities
            foreach (Info entity in chunk.DynamicEntity)
            {
                if (IsEntityInRange(Vector3Int.FloorToInt(entity.position), minX, minY, minZ, maxX, maxY, maxZ))
                { 
                    info = (Info)Helper.Clone(entity);
                    info.position = entity.position - new Vector3Int(minX, minY, minZ);
                    setPiece.DynamicEntity.Add(info);
                }
            }
        }

        return setPiece;
    }
    
    public static void Paste(Vector3Int position, Chunk setPiece)
    {
        Info info;
        Vector3Int chunkPos, worldPos;
        
        foreach (Info entity in setPiece.StaticEntity)
        {
            worldPos = position + Vector3Int.FloorToInt(entity.position);
            if (World.IsInWorldBounds(worldPos))
            { 
                chunkPos = World.GetChunkCoordinate(worldPos); 
                if (World.Inst[chunkPos] == null) Gen.Generate(chunkPos);
                info = (Info)Helper.Clone(entity);
                info.position += position;
                World.Inst[chunkPos].StaticEntity.Add(info);
            } 
        }
 
        foreach (Info entity in setPiece.DynamicEntity)
        { 
            worldPos = position + Vector3Int.FloorToInt(entity.position);
            if (World.IsInWorldBounds(worldPos))
            { 
                chunkPos = World.GetChunkCoordinate(worldPos);
                if (World.Inst[chunkPos] == null) Gen.Generate(chunkPos);
                info = (Info)Helper.Clone(entity);
                info.position += position;
                World.Inst[chunkPos].DynamicEntity.Add(info);
            }  
        }
 
        for (int x = 0; x < setPiece.size; x++)
        {
            for (int y = 0; y < setPiece.size; y++)
            {
                for (int z = 0; z < setPiece.size; z++)
                {
                    int blockID = setPiece[x, y, z];
                    if (blockID != 0)
                    {
                        worldPos = new Vector3Int(position.x + x, position.y + y, position.z + z);
                        if (World.IsInWorldBounds(worldPos))
                        {
                            chunkPos = World.GetChunkCoordinate(worldPos);
                            if (World.Inst[chunkPos] == null) Gen.Generate(chunkPos);
                            World.Inst[chunkPos][World.GetBlockCoordinate(worldPos)] = blockID; 
                        } 
                    }
                }
            }
        }
    }

    private static bool IsEntityInRange(Vector3Int coord, int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        return coord.x >= minX && coord.x <= maxX &&
               coord.y >= minY && coord.y <= maxY &&
               coord.z >= minZ && coord.z <= maxZ;
    }
    
}
