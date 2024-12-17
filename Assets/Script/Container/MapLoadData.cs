using Unity.Collections;
using UnityEngine;

public class MapLoadData
{
    private static int _size = World.ChunkSize + 2;

    public static NativeMap3D<int> Create(Vector3Int coordinate)
    {
        NativeMap3D<int> _map = new NativeMap3D<int>(_size, Allocator.TempJob);

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
        return _map;



        void SetMiddle(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y, coordinate.z];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    for (int z = 0; z < World.ChunkSize; z++)
                    {
                        _map[x, y, z] = chunkData.Map[x, y, z];
                    }
                }
            }
        }

        void SetPX(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize, coordinate.y, coordinate.z];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    _map[World.ChunkSize, y, z] = chunkData.Map[0, y, z];
                }
            }
        }

        void SetNX(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize, coordinate.y, coordinate.z];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    _map[-1, y, z] = chunkData.Map[World.ChunkSize - 1, y, z];
                }
            }
        }

        void SetPY(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y + World.ChunkSize, coordinate.z];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    _map[x, World.ChunkSize, z] = chunkData.Map[x, 0, z];
                }
            }
        }

        void SetNY(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y - World.ChunkSize, coordinate.z];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    _map[x, -1, z] = chunkData.Map[x, World.ChunkSize - 1, z];
                }
            }
        }

        void SetPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y, coordinate.z + World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    _map[x, y, World.ChunkSize] = chunkData.Map[x, y, 0];
                }
            }
        }

        void SetNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y, coordinate.z - World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    _map[x, y, -1] = chunkData.Map[x, y, World.ChunkSize - 1];
                }
            }
        }

        // Methods to set the edges
        void SetEdgePXNY(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                _map[World.ChunkSize, -1, z] = chunkData.Map[0, World.ChunkSize - 1, z];
            }
        }

        void SetEdgePXPY(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                _map[World.ChunkSize, World.ChunkSize, z] = chunkData.Map[0, 0, z];
            }
        }

        void SetEdgePXPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize, coordinate.y,
                coordinate.z + World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                _map[World.ChunkSize, y, World.ChunkSize] = chunkData.Map[0, y, 0];
            }
        }

        void SetEdgePXNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize, coordinate.y,
                coordinate.z - World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                _map[World.ChunkSize, y, -1] = chunkData.Map[0, y, World.ChunkSize - 1];
            }
        }

        void SetEdgeNXNY(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                _map[-1, -1, -1] = chunkData.Map[World.ChunkSize - 1, World.ChunkSize - 1, z];
            }
        }

        void SetEdgeNXPY(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z];
            for (int z = 0; z < World.ChunkSize; z++)
            {
                _map[-1, World.ChunkSize, z] = chunkData.Map[World.ChunkSize - 1, 0, z];
            }
        }

        void SetEdgeNXPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize, coordinate.y,
                coordinate.z + World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                _map[-1, y, World.ChunkSize] = chunkData.Map[World.ChunkSize - 1, y, 0];
            }
        }

        void SetEdgeNXNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize, coordinate.y,
                coordinate.z - World.ChunkSize];
            for (int y = 0; y < World.ChunkSize; y++)
            {
                _map[-1, y, -1] = chunkData.Map[World.ChunkSize - 1, y, World.ChunkSize - 1];
            }
        }

        void SetEdgePYNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y + World.ChunkSize,
                coordinate.z - World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                _map[x, World.ChunkSize, -1] = chunkData.Map[x, 0, World.ChunkSize - 1];
            }
        }

        void SetEdgePYPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y + World.ChunkSize,
                coordinate.z + World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                _map[x, World.ChunkSize, World.ChunkSize] = chunkData.Map[x, 0, 0];
            }
        }

        void SetEdgeNYNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y - World.ChunkSize,
                coordinate.z - World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                _map[x, -1, -1] = chunkData.Map[x, World.ChunkSize - 1, World.ChunkSize - 1];
            }
        }

        void SetEdgeNYPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x, coordinate.y - World.ChunkSize,
                coordinate.z + World.ChunkSize];
            for (int x = 0; x < World.ChunkSize; x++)
            {
                _map[x, -1, World.ChunkSize] = chunkData.Map[x, World.ChunkSize - 1, 0];
            }
        }

        // Methods to set the corners
        void SetCornerNNN(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z - World.ChunkSize];
            _map[-1, -1, -1] = chunkData.Map[World.ChunkSize - 1, World.ChunkSize - 1,
                World.ChunkSize - 1];
        }

        void SetCornerNNP(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z + World.ChunkSize];
            _map[0, -1, World.ChunkSize] =
                chunkData.Map[World.ChunkSize - 1, World.ChunkSize - 1, 0];
        }

        void SetCornerNPN(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z - World.ChunkSize];
            _map[-1, World.ChunkSize, -1] =
                chunkData.Map[World.ChunkSize - 1, 0, World.ChunkSize - 1];
        }

        void SetCornerNPP(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x - World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z + World.ChunkSize];
            _map[-1, World.ChunkSize, World.ChunkSize] = chunkData.Map[World.ChunkSize - 1, 0, 0];
        }

        void SetCornerPNN(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z - World.ChunkSize];
            _map[World.ChunkSize, -1, -1] =
                chunkData.Map[0, World.ChunkSize - 1, World.ChunkSize - 1];
        }

        void SetCornerPNP(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize,
                coordinate.y - World.ChunkSize, coordinate.z + World.ChunkSize];
            _map[World.ChunkSize, -1, World.ChunkSize] = chunkData.Map[0, World.ChunkSize - 1, 0];
        }

        void SetCornerPPN(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z - World.ChunkSize];
            _map[World.ChunkSize, World.ChunkSize, -1] = chunkData.Map[0, 0, World.ChunkSize - 1];
        }

        void SetCornerPPP(Vector3Int coordinate)
        {
            ChunkData chunkData = World.world[coordinate.x + World.ChunkSize,
                coordinate.y + World.ChunkSize, coordinate.z + World.ChunkSize];
            _map[World.ChunkSize, World.ChunkSize, World.ChunkSize] = chunkData.Map[0, 0, 0];
        }

    }
}