using UnityEngine;

[System.Serializable]
public class SVector3Int
{
    public int x;
    public int y;
    public int z;
    
    public SVector3Int() {}
    public SVector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SVector3Int(Vector3Int vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    } 
    
    public void Set(Vector3Int vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }
}