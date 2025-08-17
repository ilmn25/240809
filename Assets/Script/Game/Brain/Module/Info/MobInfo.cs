using System;
using UnityEngine;

// public class ActionTask
// {
//     [NonSerialized] public IActionType ActionType;
//     [NonSerialized] public Info SourceInfo; 
//     [NonSerialized] public Info TargetInfo; 
//
//     public Vector3 GetPosition()
//     { 
//         return TargetInfo.position;
//     }
// }

public enum IActionType {Follow, Interact, Hit, Dig, PickUp}
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
    public HitboxType TargetHitboxType;
 
    [NonSerialized] public Item Equipment;
    
    [NonSerialized] public Info Target;
    [NonSerialized] public IActionType ActionType;
    
    [NonSerialized] public bool FaceTarget;
    [NonSerialized] public Vector3 AimPosition; 
    [NonSerialized] public PathingStatus PathingStatus = PathingStatus.Pending; 
 
    protected override void OnUpdate()
    { 
        base.OnUpdate();
        if (Target != null)
        {
            TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.position) - 
                              Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
        }
    } 

    public void SetEquipment(String stringID)
    { 
        if (stringID != null)
        { 
            if (Equipment == null || Equipment.StringID != stringID)
            {
                Equipment = Item.GetItem(stringID);
                SpriteTool.gameObject.SetActive(true);
                SpriteTool.localPosition = new Vector3(Equipment.HoldoutOffset.x, Equipment.HoldoutOffset.y, 0);
                SpriteTool.localRotation = Quaternion.Euler(0, 0, Equipment.RotationOffset);
                SpriteToolRenderer.sprite = Cache.LoadSprite("sprite/" + Equipment.StringID);
                SpriteToolTrack.transform.localScale = Vector3.one * Equipment.Scale;
                Machine.SetState<EquipSelectState>();
            } 
        }
        else
        {
            Equipment = null;
            SpriteTool.gameObject.SetActive(false);
        }
    }
}