using UnityEngine;

public class GenTaskThrone : WorldGen
{
    private static int _id;
    private static int DirtBlock => _id == 0 ? Block.ConvertID(global::ID.DirtBlock) : _id;
    private static SerializableChunk Throne = SetPiece.LoadSetPieceFile("Throne");
    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        if (UnityEngine.Random.Range(0, 10) != 0) return; 
        for (int x = 0; x < World.ChunkSize - 6; x++)
        {
            for (int y = 0; y < World.ChunkSize - 1; y++)
            { 
                for (int z = 0; z < World.ChunkSize - 6; z++)
                {
                    if (CurrentChunk[x, y, z] == DirtBlock && CurrentChunk[x, y + 1, z] == 0 && 
                        CurrentChunk[x + 6, y, z + 6] == DirtBlock && CurrentChunk[x + 6, y + 1, z + 6] == 0)
                    {
                        SetPiece.Paste(CurrentCoordinate + new Vector3Int(x, y, z), Throne);
                        return;
                    }
                }
            }
        }
    }
}
public class GenTaskHouse : WorldGen
{
    private static int _id;
    private static int DirtBlock => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id; 
    private static SerializableChunk House = SetPiece.LoadSetPieceFile("House");
    
    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        if (UnityEngine.Random.Range(0, 15) != 0) return; 
        for (int x = 0; x < World.ChunkSize - 9; x++)
        {
            for (int y = 0; y < World.ChunkSize - 1; y++)
            { 
                for (int z = 0; z < World.ChunkSize - 9; z++)
                {
                    if (CurrentChunk[x, y, z] == DirtBlock && CurrentChunk[x, y + 1, z] == 0 && 
                        CurrentChunk[x + 9, y, z + 9] == DirtBlock && CurrentChunk[x + 9, y + 1, z + 9] == 0)
                    {
                        SetPiece.Paste(CurrentCoordinate + new Vector3Int(x, y, z), House);
                        return;
                    }
                }
            }
        }
    }
}