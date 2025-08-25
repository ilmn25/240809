using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SetPiece
{
    
    public static Vector3Int Pos1;
    public static Vector3Int Pos2;
     
    public static void SaveSetPieceFile(SerializableChunk setPiece, string fileName)
    {
        string json = JsonConvert.SerializeObject(setPiece, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        string path = Path.Combine(Application.dataPath, "Resources/Set", fileName + ".json");
        File.WriteAllText(path, json);
    }

    public static SerializableChunk LoadSetPieceFile(string fileName)
    { 
        TextAsset textAsset = Resources.Load<TextAsset>("Set/" + fileName);
        return JsonConvert.DeserializeObject<SerializableChunk>(textAsset.text, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
    
    public static SerializableChunk Copy()
    { 
        int minX = Mathf.Min(Pos1.x, Pos2.x);
        int minY = Mathf.Min(Pos1.y, Pos2.y);
        int minZ = Mathf.Min(Pos1.z, Pos2.z);
        int maxX = Mathf.Max(Pos1.x, Pos2.x);
        int maxY = Mathf.Max(Pos1.y, Pos2.y);
        int maxZ = Mathf.Max(Pos1.z, Pos2.z);

        SerializableChunk setPiece = new SerializableChunk(Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ) + 1);
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
                    entity.position -= new Vector3Int(minX, minY, minZ);
                    setPiece.StaticEntity.Add(entity.ToSetPieceInfo());  
                }
            }

            // Check and add dynamic entities
            foreach (Info entity in chunk.DynamicEntity)
            {
                if (IsEntityInRange(Vector3Int.FloorToInt(entity.position), minX, minY, minZ, maxX, maxY, maxZ))
                {
                    entity.position -= new Vector3Int(minX, minY, minZ);
                    setPiece.DynamicEntity.Add(entity.ToSetPieceInfo()); 
                }
            }
        }

        return setPiece;
    }
    
    public static void Paste(Vector3Int position, SerializableChunk setPiece)
    {  
        Vector3Int chunkPos, worldPos;
        
        foreach (SetEntity entity in setPiece.StaticEntity)
        {
            worldPos = position + entity.position;
            if (World.IsInWorldBounds(worldPos))
            {
                chunkPos = World.GetChunkCoordinate(worldPos); 
                if (World.Inst[chunkPos] == null) WorldGen.Generate(chunkPos);
                World.Inst[chunkPos].StaticEntity.Add(
                    Entity.CreateInfo(entity.id, worldPos));
            } 
        }
 
        foreach (SetEntity entity in setPiece.DynamicEntity)
        { 
            worldPos = position + entity.position;
            if (World.IsInWorldBounds(worldPos))
            { 
                chunkPos = World.GetChunkCoordinate(worldPos);
                if (World.Inst[chunkPos] == null) WorldGen.Generate(chunkPos);
                World.Inst[chunkPos].DynamicEntity.Add(
                    Entity.CreateInfo(entity.id, worldPos));
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
                            if (World.Inst[chunkPos] == null) WorldGen.Generate(chunkPos);
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
