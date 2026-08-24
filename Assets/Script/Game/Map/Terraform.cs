using System.Collections.Generic;
using UnityEngine;

public static class Terraform
{
    private const int PreviewSpeed = 10; 

    public static readonly List<Vector3Int> PendingBlocks = new ();
    private static GameObject _blockObj;
    private static bool _overlayEnabled;
    private static Vector3Int _coordinate;
    public static ID Target; 

    public static void Initialize()
    {
        _blockObj = ObjectPool.GetObject(ID.BlockPrefab);
        _blockObj.SetActive(false); 
    }
    
    public static void BlockUpdate(ID target = ID.Null)
    {
        // Mining tools place the mining box directly.
        if (Aim.IsMiningTool())
            target = ID.MiningBox;

        Target = target;
        ConfigurePreview(_blockObj, target);
    }

    private static void ConfigurePreview(GameObject obj, ID target)
    {
        Item held = Inventory.CurrentItemData;
        if (held == null)
        {
            obj.SetActive(false);
            return;
        }

        bool mining = target == ID.MiningBox;
        ID blockID;
        float scale;
        bool overlay;
        if (mining) { blockID = ID.OverlayBlock; scale = 1.04f; overlay = true; }
        else if (held.Type == ItemType.Structure) { blockID = ID.OverlayBlock; scale = 1f; overlay = true; }
        else if (held.Type == ItemType.Block) { blockID = held.ID; scale = 1f; overlay = false; }
        else { obj.SetActive(false); return; }

        string key = overlay ? "overlay" : held.ID.ToString();
        if (obj.name != key)
        {
            obj.name = key;
            BlockPreview.Apply(obj, blockID, scale);
        }
        // The aim toggle only gates the mining overlay; build previews always show.
        obj.SetActive(mining ? _overlayEnabled : true);
    }
    
    public static void Update()
    {
        // Placing is skipped while hovering a slot so inventory management
        // (pick up/swap) takes priority — the preview hides too.
        if (GUIStorage.HoveringSlot)
        {
            _blockObj.SetActive(false);
            return;
        }

        // B-key toggles the mining aim overlay. Build previews are always shown.
        if (Control.Inst.terraform.KeyDown())
            _overlayEnabled = !_overlayEnabled;

        Item held = Inventory.CurrentItemData;
        bool overMap = Helper.isLayer(Control.MouseLayer, Main.IndexMap) &&
                       Main.PlayerInfo.Machine.IsCurrentState<DefaultState>();
        bool usable = held != null && (Aim.IsMiningTool() || held.Type is ItemType.Block or ItemType.Structure);

        bool mining = Target == ID.MiningBox;
        bool aimVisible = mining ? _overlayEnabled : true;

        // The preview object snaps to the targeted cell whenever visible, a usable item
        // is held, and the cursor is over the world (map or a structure). It stays
        // visible during swings too (no DefaultState requirement).
        Vector3Int cell = Aim.Cell();
        if (aimVisible && usable && Control.MouseLayer != -1 && World.IsInWorldBounds(cell))
        {
            _blockObj.SetActive(true);
            Position(_blockObj, cell);
        }
        else
        {
            _blockObj.SetActive(false);
        }

        // Placing requires the cursor preview to be on. For mining that means the
        // toggle must be on; build always places.
        bool canPlace = (mining ? _overlayEnabled : true) && Target != ID.Null && overMap && HandleCoord();
        if (canPlace && Control.Inst.ActionPrimary.Key())
            Place(held);
    }

    private static void Place(Item held)
    {
        if (Target == ID.MiningBox)
        {
            // Placing a mining box is a marker drop, not a swing — mining happens
            // with the follow-up swing.
            SpawnBlock(false);
            return;
        }

        Main.PlayerInfo.Machine.SetState<MobAttackSwing>();
        Audio.PlaySFX(held.Sfx);
        SpawnBlock(held.Type == ItemType.Structure);
    }

    private static void Position(GameObject obj, Vector3Int cell)
    {
        obj.transform.position = Vector3.Lerp(obj.transform.position,
            cell + new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
    }
  
    public static void SpawnBlock(bool isStructure)
    {
        if (isStructure)
        {
            // Structures place directly — no build phase.
            Entity.Spawn(Target, _coordinate);
            Tutorial.OnPlaced(Target);
            RemoveHeldItem();
            return;
        }

        if (Main.CreativeMode)
        {
            if (Target == ID.MiningBox)
                World.SetBlock(_coordinate);
            else
            {
                Main.PlayerInfo.Storage.CreateAndAddItem(Target);
                World.SetBlock(_coordinate, Block.ConvertID(Target)); 
            }
            return;
        }
        
        if (Target == ID.MiningBox)
        {
            // The mining box is an overlay on an existing block — refuse to place it
            // on empty/unregistered ground so BlockInfo.Initialize doesn't crash.
            if (Block.GetBlock(World.GetBlock(_coordinate)) == null)
                return;
            Entity.Spawn(ID.MiningBox, _coordinate);
            return;
        }

        Tutorial.OnBlockPlaced();
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
        Vector3Int adjustedPoint = Aim.Cell();
        if (PendingBlocks.Contains(adjustedPoint) || !Scene.InPlayerBlockRange(adjustedPoint, 3) ||
            !World.IsInWorldBounds(adjustedPoint)) return false; 
        _coordinate = adjustedPoint;
        return true;
    } 
}
    