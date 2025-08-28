using System;
using UnityEngine;
using UnityEngine.Serialization;

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
    public int PointLostDistance = 5;
    public int NormalSkipAmount = 1;
    public bool mustLandFirst = false;
    public HitboxType targetHitboxType;
 
    [NonSerialized] public ItemSlot Equipment;
    
    [NonSerialized] public Info Target;
    [NonSerialized] public IActionType ActionType;
    
    [NonSerialized] public bool FaceTarget;
    [NonSerialized] public Vector3 AimPosition; 
    [NonSerialized] public PathingStatus PathingStatus = PathingStatus.Pending;

    public void CancelTarget()
    {
        Target = null; 
        PathingStatus = PathingStatus.Stuck;
        Direction = Vector3.zero;            
        Machine.SetState<DefaultState>();
    }
    
    protected override void OnUpdate()
    { 
        base.OnUpdate();
        if (Target != null)
        {
            if (Target.Destroyed)
            {
                CancelTarget();
            }
            else
            {
                TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.position) - 
                                   Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
            }
        }
    }

    public Vector3 GetDirection()
    {
        Vector3 direction = SpriteToolTrack.right;
        if (SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        return direction;
    }
    public void SetEquipment(ItemSlot target)
    { 
        if (target != null)
        { 
            if (Equipment == null || Equipment != target)
            {
                Equipment = target;
                SpriteTool.gameObject.SetActive(true);
                SpriteTool.localPosition = new Vector3(Equipment.Info.HoldoutOffset.x, Equipment.Info.HoldoutOffset.y, 0);
                SpriteTool.localRotation = Quaternion.Euler(0, 0, Equipment.Info.RotationOffset);
                SpriteToolRenderer.sprite = Cache.LoadSprite("Sprite/" + Equipment.Info.ID);
                SpriteToolTrack.transform.localScale = Vector3.one * Equipment.Info.Scale;
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