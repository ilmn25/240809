using System;
using UnityEngine;

public enum IActionTarget {Primary, Secondary, Hit}
[System.Serializable]
public class MobInfo : DynamicInfo
{ 
    public int DistAttack = 2;
    public int DistAlert = 10;
    public int DistDisengage = 20; 
    
    public int DistEscape = 20;
    public int DistRoam = 10;
    public int DistStrafe = 2;
    
    public int PathJump = 1;
    public int PathHeight = 1;
    public int PathFall = 15;
    public int PathAir = 4;
    public int PathAmount = 3000;
    public int MaxStuckCount = 250;
    public string EquipmentId;
    public HitboxType TargetHitboxType;
 
    [NonSerialized] public Item Equipment;
    [NonSerialized] public Transform Target;
    [NonSerialized] public IAction Action;
    [NonSerialized] public IActionTarget ActionTarget;
    [NonSerialized] public bool FaceTarget;
    [NonSerialized] public Vector3 AimPosition; 
    [NonSerialized] public PathingStatus PathingStatus = PathingStatus.Pending; 
 
    protected override void OnUpdate()
    { 
        base.OnUpdate();
        if (Target)
        {
            TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.transform.position) - 
                              Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
        }
    } 

    public void SetEquipment(String stringID)
    {
        Item item = null;
        if (stringID != null) item = Item.GetItem(stringID);
        if (item != null)
        {
            if (Equipment == null || Equipment != item)
            {
                EquipmentId = stringID;
                Equipment = item;
                SpriteTool.gameObject.SetActive(true);
                SpriteTool.localPosition = new Vector3(item.HoldoutOffset.x, item.HoldoutOffset.y, 0);
                SpriteTool.localRotation = Quaternion.Euler(0, 0, Equipment.RotationOffset);
                SpriteToolRenderer.sprite = Cache.LoadSprite("sprite/" + item.StringID);
                SpriteToolTrack.transform.localScale = Vector3.one * item.Scale;
                Machine.SetState<EquipSelectState>();
            } 
        }
        else
        {
            EquipmentId = null;
            Equipment = null;
            SpriteTool.gameObject.SetActive(false);
        }
    }
}