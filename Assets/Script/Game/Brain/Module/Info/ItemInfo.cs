using UnityEngine;

[System.Serializable]
public class ItemInfo : Info
{
    public override void Update()
    {
        if (Machine) position = Machine.transform.position;
    }

    public void OnActionSecondary(Info info)
    {        
        if (Vector3.Distance(position, info.Machine.transform.position) < 3f) 
        { 
            Audio.PlaySFX("pick_up", 0.4f);
            ((PlayerInfo)info).Storage.AddItem(stringID);
            EntityMachine.Delete();
        }
    }
}