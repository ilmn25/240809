using Unity.Collections;
using UnityEngine;

public class MeshLoadData
{
    public static NativeMap3D<int> Create(Vector3Int coordinate)
    {
        NativeMap3D<int> map = new NativeMap3D<int>(World.ChunkSize + 2, Allocator.TempJob);

        // Call methods to set faces
        SetPX(coordinate);
        SetNX(coordinate);
        SetPY(coordinate);
        SetNY(coordinate);
        SetPZ(coordinate);
        SetNZ(coordinate);

        // Call methods to set edges
        SetEdgePXNY(coordinate);
        SetEdgePXPY(coordinate);
        SetEdgePXPZ(coordinate);
        SetEdgePXNZ(coordinate);
        SetEdgeNXNY(coordinate);
        SetEdgeNXPY(coordinate);
        SetEdgeNXPZ(coordinate);
        SetEdgeNXNZ(coordinate);
        SetEdgePYNZ(coordinate);
        SetEdgePYPZ(coordinate);
        SetEdgeNYNZ(coordinate);
        SetEdgeNYPZ(coordinate);

        // Call methods to set corners
        SetCornerNNN(coordinate);
        SetCornerNNP(coordinate);
        SetCornerNPN(coordinate);
        SetCornerNPP(coordinate);
        SetCornerPNN(coordinate);
        SetCornerPNP(coordinate);
        SetCornerPPN(coordinate);
        SetCornerPPP(coordinate);

        SetMiddle(coordinate);
        return map;



        void SetMiddle(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y, coordinate.z];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    for (int z = 0; z < World.ChunkSize; z++)
                    {
                        map[x, y, z] = chunk.Map[x, y, z];
                    }
                }
            }
        }

        void SetPX(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize, coordinate.y, coordinate.z];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    map[World.ChunkSize, y, z] = chunk.Map[0, y, z];
                }
            }
        }

        void SetNX(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize, coordinate.y, coordinate.z];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    map[-1, y, z] = chunk.Map[World.ChunkSize - 1, y, z];
                }
            }
        }

        void SetPY(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y + World.ChunkSize, coordinate.z];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    map[x, World.ChunkSize, z] = chunk.Map[x, 0, z];
                }
            }
        }

        void SetNY(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y - World.ChunkSize, coordinate.z];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    map[x, -1, z] = chunk.Map[x, World.ChunkSize - 1, z];
                }
            }
        }

        void SetPZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y, coordinate.z + World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    map[x, y, World.ChunkSize] = chunk.Map[x, y, 0];
                }
            }
        }

        void SetNZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y, coordinate.z - World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    map[x, y, -1] = chunk.Map[x, y, World.ChunkSize - 1];
                }
            }
        }

        // Methods to set the edges
        void SetEdgePXNY(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                map[World.ChunkSize, -1, z] = chunk.Map[0, World.ChunkSize - 1, z];
            }
        }

        void SetEdgePXPY(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                map[World.ChunkSize, World.ChunkSize, z] = chunk.Map[0, 0, z];
            }
        }

        void SetEdgePXPZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize, coordinate.y,
                coordinate.z + World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                map[World.ChunkSize, y, World.ChunkSize] = chunk.Map[0, y, 0];
            }
        }

        void SetEdgePXNZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize, coordinate.y,
                coordinate.z - World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                map[World.ChunkSize, y, -1] = chunk.Map[0, y, World.ChunkSize - 1];
            }
        }

        void SetEdgeNXNY(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                map[-1, -1, -1] = chunk.Map[World.ChunkSize - 1, World.ChunkSize - 1, z];
            }
        }

        void SetEdgeNXPY(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                map[-1, World.ChunkSize, z] = chunk.Map[World.ChunkSize - 1, 0, z];
            }
        }

        void SetEdgeNXPZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize, coordinate.y,
                coordinate.z + World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                map[-1, y, World.ChunkSize] = chunk.Map[World.ChunkSize - 1, y, 0];
            }
        }

        void SetEdgeNXNZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize, coordinate.y,
                coordinate.z - World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                map[-1, y, -1] = chunk.Map[World.ChunkSize - 1, y, World.ChunkSize - 1];
            }
        }

        void SetEdgePYNZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y + World.ChunkSize,
                coordinate.z - World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                map[x, World.ChunkSize, -1] = chunk.Map[x, 0, World.ChunkSize - 1];
            }
        }

        void SetEdgePYPZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y + World.ChunkSize,
                coordinate.z + World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                map[x, World.ChunkSize, World.ChunkSize] = chunk.Map[x, 0, 0];
            }
        }

        void SetEdgeNYNZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y - World.ChunkSize,
                coordinate.z - World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                map[x, -1, -1] = chunk.Map[x, World.ChunkSize - 1, World.ChunkSize - 1];
            }
        }

        void SetEdgeNYPZ(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x, coordinate.y - World.ChunkSize,
                coordinate.z + World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                map[x, -1, World.ChunkSize] = chunk.Map[x, World.ChunkSize - 1, 0];
            }
        }

        // Methods to set the corners
        void SetCornerNNN(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z - World.ChunkSize];
            map[-1, -1, -1] = chunk.Map[World.ChunkSize - 1, World.ChunkSize - 1,
                World.ChunkSize - 1];
        }

        void SetCornerNNP(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z + World.ChunkSize];
            map[0, -1, World.ChunkSize] =
                chunk.Map[World.ChunkSize - 1, World.ChunkSize - 1, 0];
        }

        void SetCornerNPN(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z - World.ChunkSize];
            map[-1, World.ChunkSize, -1] =
                chunk.Map[World.ChunkSize - 1, 0, World.ChunkSize - 1];
        }

        void SetCornerNPP(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x - World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z + World.ChunkSize];
            map[-1, World.ChunkSize, World.ChunkSize] = chunk.Map[World.ChunkSize - 1, 0, 0];
        }

        void SetCornerPNN(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z - World.ChunkSize];
            map[World.ChunkSize, -1, -1] =
                chunk.Map[0, World.ChunkSize - 1, World.ChunkSize - 1];
        }

        void SetCornerPNP(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z + World.ChunkSize];
            map[World.ChunkSize, -1, World.ChunkSize] = chunk.Map[0, World.ChunkSize - 1, 0];
        }

        void SetCornerPPN(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z - World.ChunkSize];
            map[World.ChunkSize, World.ChunkSize, -1] = chunk.Map[0, 0, World.ChunkSize - 1];
        }

        void SetCornerPPP(Vector3Int coordinate)
        {
            Chunk chunk = World.Inst[coordinate.x + World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z + World.ChunkSize];
            map[World.ChunkSize, World.ChunkSize, World.ChunkSize] = chunk.Map[0, 0, 0];
        }

    }
}