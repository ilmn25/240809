using UnityEngine;

public class GenTaskSet : WorldGen
{
    private static int _id;
    private static int DirtBlock => _id == 0 ? Block.ConvertID(global::ID.DirtBlock) : _id; 
    
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
                        SetPiece.LoadAndPaste(CurrentCoordinate + new Vector3Int(x, y, z), "Throne");
                        return;
                    }
                }
            }
        }
    }
}