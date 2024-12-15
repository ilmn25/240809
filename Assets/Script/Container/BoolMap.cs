using System.Collections;

public class BoolMap
{
    private BitArray _bitMap;
    private int _xSize, _ySize, _zSize;

    public BoolMap(int x, int y, int z)
    {
        _xSize = x;
        _ySize = y;
        _zSize = z;
        _bitMap = new BitArray(x * y * z);
    }

    public bool this[int x, int y, int z]
    {
        get
        {
            int index = GetIndex(x, y, z);
            return _bitMap[index];
        }
        set
        {
            int index = GetIndex(x, y, z);
            _bitMap[index] = value;
        }
    }

    private int GetIndex(int x, int y, int z)
    {
        return x + _xSize * (y + _ySize * z);
    }

    public bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < _xSize &&
               y >= 0 && y < _ySize &&
               z >= 0 && z < _zSize;
    }
}