using UnityEngine;

public class GenTaskThrone : Gen
{
    private static int _id;
    private static int DirtBlock => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id;
    private static readonly Chunk Throne = SetPiece.LoadSetPieceFile("Throne");
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (UnityEngine.Random.Range(0, 10) != 0) return; 
        for (int x = 0; x < World.ChunkSize - 6; x++)
        {
            for (int y = 0; y < World.ChunkSize - 1; y++)
            { 
                for (int z = 0; z < World.ChunkSize - 6; z++)
                {
                    if (currentChunk[x, y, z] == DirtBlock && currentChunk[x, y + 1, z] == 0 && 
                        currentChunk[x + 6, y, z + 6] == DirtBlock && currentChunk[x + 6, y + 1, z + 6] == 0)
                    {
                        SetPiece.Paste(currentCoordinate + new Vector3Int(x, y, z), Throne);
                        return;
                    }
                }
            }
        }
    }
}
public class GenTaskHouse : Gen
{
    private static int _id;
    private static int DirtBlock => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id; 
    private static readonly Chunk House = SetPiece.LoadSetPieceFile("House");
    
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (UnityEngine.Random.Range(0, 15) != 0) return; 
        for (int x = 0; x < World.ChunkSize - 9; x++)
        {
            for (int y = 0; y < World.ChunkSize - 1; y++)
            { 
                for (int z = 0; z < World.ChunkSize - 9; z++)
                {
                    if (currentChunk[x, y, z] == DirtBlock && currentChunk[x, y + 1, z] == 0 && 
                        currentChunk[x + 9, y, z + 9] == DirtBlock && currentChunk[x + 9, y + 1, z + 9] == 0)
                    {
                        SetPiece.Paste(currentCoordinate + new Vector3Int(x, y, z), House);
                        return;
                    }
                }
            }
        }
    }
}