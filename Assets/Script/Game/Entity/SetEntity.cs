using UnityEngine;

[System.Serializable]
public class SetEntity
{
    public string stringID;
    public Vector3Int position;

    public Info ToInfo()
    {
        return Entity.CreateInfo(stringID, position);
    }
} 
    