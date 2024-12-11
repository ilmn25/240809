using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SetPiece
{
    
    private static Vector3Int _positionA;
    private static Vector3Int _positionB;
    
    public static void Update()
    { 
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Lib.Log("anchor A set");
            _positionA = Vector3Int.FloorToInt(Game.Player.transform.position);
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Lib.Log("anchor B set");
            _positionB = Vector3Int.FloorToInt(Game.Player.transform.position);
        } 
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Lib.Log("exported to file");
            SaveSetPieceFile(CopySetPiece(), "tree_a");
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Lib.Log("imported to world"); 
            PasteSetPiece(Vector3Int.FloorToInt(Game.Player.transform.position), LoadSetPieceFile("tree_a"));
        }
    }
     
    public static void SaveSetPieceFile(SerializableChunkData setPiece, string fileName)
    {
        string json = JsonUtility.ToJson(setPiece);
        string path = Path.Combine(Application.dataPath, "Resources/set", fileName + ".json");
        File.WriteAllText(path, json);
    }

    public static SerializableChunkData LoadSetPieceFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>("set/" + fileName);
        return JsonUtility.FromJson<SerializableChunkData>(textAsset.text);
    }
    
    public static SerializableChunkData CopySetPiece()
    {
        int minX = Mathf.Min(_positionA.x, _positionB.x);
        int minY = Mathf.Min(_positionA.y, _positionB.y);
        int minZ = Mathf.Min(_positionA.z, _positionB.z);
        int maxX = Mathf.Max(_positionA.x, _positionB.x);
        int maxY = Mathf.Max(_positionA.y, _positionB.y);
        int maxZ = Mathf.Max(_positionA.z, _positionB.z);

        SerializableChunkData setPiece = new SerializableChunkData(Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ) + 1);
        Vector3Int min = new Vector3Int(minX, minY, minZ);

        List<Vector3Int> scannedChunks = new List<Vector3Int>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int worldPos = new Vector3Int(x, y, z);
                    Vector3Int localPos = worldPos - min;

                    Vector3Int chunkPos = WorldSingleton.GetChunkCoordinate(worldPos);
                    int localChunkX = worldPos.x - chunkPos.x;
                    int localChunkY = worldPos.y - chunkPos.y;
                    int localChunkZ = worldPos.z - chunkPos.z;

                    ChunkData chunk = WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z];
                    setPiece[localPos.x, localPos.y, localPos.z] = chunk.Map[localChunkX, localChunkY, localChunkZ];
                    
                    if (!scannedChunks.Contains(chunkPos))
                    {
                        scannedChunks.Add(chunkPos);
                    } 
                }
            }
        }

        foreach (var chunkCoord in scannedChunks)
        {
            ChunkData chunk = WorldSingleton.World[chunkCoord.x, chunkCoord.y, chunkCoord.z];
            
            // Check and add static entities
            foreach (var entity in chunk.StaticEntity)
            {
                if (IsEntityInRange(entity.Position.ToVector3Int() + chunkCoord, minX, minY, minZ, maxX, maxY, maxZ))
                {
                    entity.Position = new SerializableVector3Int(entity.Position.ToVector3Int() + chunkCoord - new Vector3Int(minX, minY, minZ));
                    setPiece.StaticEntity.Add(entity); 
                }
            }

            // Check and add dynamic entities
            foreach (var entity in chunk.DynamicEntity)
            {
                if (IsEntityInRange(entity.Position.ToVector3Int() + chunkCoord, minX, minY, minZ, maxX, maxY, maxZ))
                {
                    entity.Position = new SerializableVector3Int(entity.Position.ToVector3Int() + chunkCoord - new Vector3Int(minX, minY, minZ));
                    setPiece.DynamicEntity.Add(entity); 
                }
            }
        }
        return setPiece; 
    }
    
    public static void PasteSetPiece(Vector3Int position, SerializableChunkData setPiece)
    { 
        int chunkSize = WorldSingleton.CHUNK_SIZE;
        Vector3Int chunkPos;
        
        foreach (var entity in setPiece.StaticEntity)
        {
            chunkPos = WorldSingleton.GetChunkCoordinate(position + entity.Position.ToVector3Int());
            entity.Position = new SerializableVector3Int(WorldSingleton.GetBlockCoordinate(position + entity.Position.ToVector3Int()));
            WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z].StaticEntity.Add(entity);
        }

        foreach (var entity in setPiece.DynamicEntity)
        {
            chunkPos = WorldSingleton.GetChunkCoordinate(position + entity.Position.ToVector3Int());
            entity.Position = new SerializableVector3Int(WorldSingleton.GetBlockCoordinate(position + entity.Position.ToVector3Int()));
            WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z].DynamicEntity.Add(entity);
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
                        Vector3Int worldPos = new Vector3Int(position.x + x, position.y + y, position.z + z);
                        chunkPos = WorldSingleton.Instance.GetRelativePosition(worldPos);
                        int localChunkX = worldPos.x % chunkSize;
                        int localChunkY = worldPos.y % chunkSize;
                        int localChunkZ = worldPos.z % chunkSize;

                        if (WorldSingleton.Instance.IsInWorldBounds(worldPos))
                            WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z].Map[localChunkX, localChunkY, localChunkZ] = blockID;
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
