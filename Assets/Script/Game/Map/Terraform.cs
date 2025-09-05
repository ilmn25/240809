using System.Collections.Generic;
using UnityEngine;

public static class Terraform
{
    private const int PreviewSpeed = 10; 
    
    public static readonly List<Vector3Int> PendingBlocks = new ();
    private static GameObject _blockObj;
    private static Vector3 _position;
    private static Vector3 _direction;
    private static Vector3Int _coordinate;
    public static ID Target; 

    public static void Initialize()
    {
        _blockObj = ObjectPool.GetObject(ID.BlockPrefab);
        _blockObj.SetActive(false); 
    }
    
    public static void BlockUpdate(ID target = ID.Null)
    {
        Target = target;
        if (Target == ID.Null)
        {
            _blockObj.SetActive(false);
        } 
        else if (Inventory.CurrentItemData.ID == ID.Blueprint)
        {
            _blockObj.SetActive(true); 
            if (_blockObj.name != "overlay")
            {
                _blockObj.name = "overlay";
                BlockPreview.Set(_blockObj, ID.OverlayBlock);
                _blockObj.transform.localScale = Vector3.one * 1.04f;
            } 
        }
        else  
        {
            _blockObj.SetActive(true);
            if (_blockObj.name != Inventory.CurrentItemData.ID.ToString())
            {
                _blockObj.name = Inventory.CurrentItemData.ID.ToString();
                BlockPreview.Set(_blockObj, Inventory.CurrentItemData.ID);
                _blockObj.transform.localScale = Vector3.one;
            }  
        }  
    }
    
    public static void Update()
    {
        if (Target == ID.Null) return;
        if (Helper.isLayer(Control.MouseLayer, Game.IndexMap) && 
            Game.PlayerInfo.Machine.IsCurrentState<DefaultState>())
        { 
            HandleCoord(); 
 
            if (Control.Inst.ActionSecondary.Key())
            {
                Game.PlayerInfo.Machine.SetState<MobAttackSwing>();
                Audio.PlaySFX(Inventory.CurrentItemData.Sfx);
                SpawnBlock();
                if (Target != ID.Blueprint) Game.PlayerInfo.storage.RemoveItem(Target);
            }
        }
        _blockObj.transform.position = Vector3.Lerp(_blockObj.transform.position, _coordinate + 
            new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
    }
  
    public static void SpawnBlock()
    {
        if (Game.BuildMode)
        {
            if (Target == ID.Blueprint)
                World.SetBlock(_coordinate);
            else
            {
                Game.PlayerInfo.storage.CreateAndAddItem(Target);
                World.SetBlock(_coordinate, Block.ConvertID(Target)); 
            }
            return;
        }
        
        Entity.Spawn(Target, _coordinate);
    }
     
    
    private static void HandleCoord()
    {
        Vector3Int adjustedPoint;
        if (Target == ID.Blueprint)
            adjustedPoint =  Vector3Int.FloorToInt(Control.MousePosition + Control.MouseDirection * 0.02f);
        else
            adjustedPoint =  Vector3Int.FloorToInt(Control.MousePosition - Control.MouseDirection * 0.02f);
  
        
        if (PendingBlocks.Contains(adjustedPoint) || !Scene.InPlayerBlockRange(adjustedPoint, 4) ||
            !World.IsInWorldBounds(adjustedPoint)) return; 
        _coordinate = adjustedPoint;
    } 
}
    