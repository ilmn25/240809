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
            return;
        }

        if (Inventory.CurrentItemData.ID == ID.ChalkPowder)
        {
            _blockObj.SetActive(true); 
            if (_blockObj.name != "overlay")
            {
                _blockObj.name = "overlay";
                BlockPreview.Set(_blockObj, ID.OverlayBlock);
                _blockObj.transform.localScale = Vector3.one * 1.04f;
            } 
        }
        else if (Inventory.CurrentItemData.Type == ItemType.Structure)
        {
            _blockObj.SetActive(true);
            if (_blockObj.name != "overlay")
            {
                _blockObj.name = "overlay";
                BlockPreview.Set(_blockObj, ID.OverlayBlock);
                _blockObj.transform.localScale = Vector3.one;
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
        // The cursor item is the held item whenever it isn't empty, so the same place
        // path works from the cursor. Placing is skipped while hovering a slot so
        // inventory management (pick up/swap) takes priority.
        if (GUIStorage.HoveringSlot) return;
        Item currentItemData = Inventory.CurrentItemData;

        if (Helper.isLayer(Control.MouseLayer, Main.IndexMap) && 
            Main.PlayerInfo.Machine.IsCurrentState<DefaultState>())
        { 
            HandleCoord(); 
 
            if (Control.Inst.ActionPrimary.Key())
            {
                bool isStructure = currentItemData.Type == ItemType.Structure;
                Main.PlayerInfo.Machine.SetState<MobAttackSwing>();
                Audio.PlaySFX(currentItemData.Sfx);
                SpawnBlock(isStructure);
            }
        }
        _blockObj.transform.position = Vector3.Lerp(_blockObj.transform.position, _coordinate + 
            new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
    }
  
    public static void SpawnBlock(bool isStructure)
    {
        if (isStructure)
        {
            // Furniture places directly — no build phase, it spawns as the real structure.
            if (Item.GetItem(Target)?.Furniture == true)
            {
                Entity.Spawn(Target, _coordinate);
                Tutorial.OnPlaced(Target);
                RemoveHeldItem();
                return;
            }
            ConstructionInfo info = (ConstructionInfo)Entity.Spawn(ID.Construction, _coordinate);
            info.structureID = Target;
            info.Health = ItemRecipe.GetRecipe(Target).Time;
            info.operationType = OperationType.Building;
            info.SfxHit = SfxID.HitMetal;
            info.SfxDestroy = SfxID.HitMetal;
            Tutorial.OnPlaced(Target);
            RemoveHeldItem();
            return;
        }

        if (Main.CreativeMode)
        {
            if (Target == ID.ChalkPowder)
                World.SetBlock(_coordinate);
            else
            {
                Main.PlayerInfo.Storage.CreateAndAddItem(Target);
                World.SetBlock(_coordinate, Block.ConvertID(Target)); 
            }
            return;
        }
        
        Tutorial.OnBlockPlaced();

        // Chalk powder is an overlay on an existing block — refuse to place it on
        // empty/unregistered ground so BlockInfo.Initialize doesn't crash.
        if (Target == ID.ChalkPowder && Block.GetBlock(World.GetBlock(_coordinate)) == null)
            return;

        Entity.Spawn(Target, _coordinate);
        RemoveHeldItem();
    }

    /// <summary>Consume one placed item from the held slot (cursor or hotbar).</summary>
    private static void RemoveHeldItem()
    {
        Inventory.CurrentItem.Stack--;
        if (Inventory.CurrentItem.Stack <= 0) Inventory.CurrentItem.clear();
        Inventory.RefreshInventory();
    }
     
    
    private static bool HandleCoord()
    {
        Vector3Int adjustedPoint;
        if (Target == ID.ChalkPowder)
            adjustedPoint =  Vector3Int.FloorToInt(Control.MousePosition + Control.MouseDirection * 0.02f);
        else
            adjustedPoint =  Vector3Int.FloorToInt(Control.MousePosition - Control.MouseDirection * 0.02f);
  
        
        if (PendingBlocks.Contains(adjustedPoint) || !Scene.InPlayerBlockRange(adjustedPoint, 4) ||
            !World.IsInWorldBounds(adjustedPoint)) return false; 
        _coordinate = adjustedPoint;
        return true;
    } 
}
    