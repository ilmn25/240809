using System.Collections.Generic;
using UnityEngine;

public static class Terraform
{
    private const int PreviewSpeed = 10; 

    public static readonly List<Vector3Int> PendingBlocks = new ();
    private static GameObject _blockObj;
    private static bool _overlayEnabled = true;
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

        // The single preview object doubles as the aim highlight, so generic tools
        // (cutting/building) that target structures also show an overlay.
        ID blockID;
        float scale;
        bool overlay;
        if (target == ID.MiningBox) { blockID = ID.OverlayBlock; scale = 1.04f; overlay = true; }
        else if (held.Type == ItemType.Structure) { blockID = ID.OverlayBlock; scale = 1f; overlay = true; }
        else if (held.Type == ItemType.Block) { blockID = held.ID; scale = 1f; overlay = false; }
        else if (held.Type == ItemType.Tool) { blockID = ID.OverlayBlock; scale = 1.04f; overlay = true; }
        else { obj.SetActive(false); return; }

        string key = overlay ? "overlay" : held.ID.ToString();
        if (obj.name != key)
        {
            obj.name = key;
            BlockPreview.Apply(obj, blockID, scale);
        }
        obj.SetActive(_overlayEnabled);
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

        // Right-click toggles the cursor overlay — but only when it wasn't used to
        // interact with something (cancel a mining box, open a chest, etc.).
        bool interacting = Control.MouseTarget != null &&
            Control.MouseTarget.GetComponentInParent<IActionSecondary>() != null;
        if (Control.Inst.ActionSecondary.KeyDown() && !interacting)
            _overlayEnabled = !_overlayEnabled;

        Item held = Inventory.CurrentItemData;
        bool overMap = Helper.isLayer(Control.MouseLayer, Main.IndexMap) &&
                       Main.PlayerInfo.Machine.IsCurrentState<DefaultState>();
        bool usable = held != null && held.Type is ItemType.Tool or ItemType.Block or ItemType.Structure;

        // The preview object snaps to the targeted cell whenever the overlay is enabled,
        // a usable item is held, and the cursor is over the world (map or a structure).
        // It stays visible during swings too (no DefaultState requirement).
        Vector3Int cell = Aim.Cell();
        if (_overlayEnabled && usable && Control.MouseLayer != -1 && World.IsInWorldBounds(cell))
        {
            _blockObj.SetActive(true);
            Position(_blockObj, cell);
        }
        else
        {
            _blockObj.SetActive(false);
        }

        // Placing (including spawning mining boxes) requires the cursor preview to be
        // on — right-clicking the overlay off also disables placement.
        bool canPlace = _overlayEnabled && Target != ID.Null && overMap && HandleCoord();
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
    