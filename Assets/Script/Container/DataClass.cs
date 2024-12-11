using Unity.Collections;

[System.Serializable]
public class Array3D<T>
{
    public T[] array;
    public int size;

    public void Initialize(int size)
    {
        this.size = size;
        array = new T[size * size * size];
    }

    public T this[int x, int y, int z]
    {
        get => array[x + size * (y + size * z)];
        set => array[x + size * (y + size * z)] = value;
    }
    public int Size => size; 
}



public struct NativeMap3D<T> where T : struct
{
    private NativeArray<T> array;
    private int size;

    public NativeMap3D(int size, Allocator allocator)
    {
        this.size = size;
        this.array = new NativeArray<T>(size * size * size, allocator);
    }

    public T this[int x, int y, int z]
    {
        get => array[(x+1) * size * size + (y+1) * size + (z+1)];
        set => array[(x+1) * size * size + (y+1) * size + (z+1)] = value;
    }

    public void Dispose()
    {
        array.Dispose();
    }
}
