using System.Collections.Generic;
using UnityEngine;

public static class Terraform
{
    private const int PreviewSpeed = 17;
    private const float Reach = 3f;

    public static readonly List<Vector3Int> PendingBlocks = new ();
    private static GameObject _blockObj;
    private static bool _overlayEnabled;
    private static ID _previewTarget;
    private static ID Target; 

    public static void Initialize()
    {
        _blockObj = ObjectPool.GetObject(ID.BlockPrefab);
        _blockObj.SetActive(false);
        _previewTarget = ID.Null;
    }
    
    private static void ConfigurePreview(Item held)
    {
        if (Target == ID.Null || _previewTarget == Target) return;
        _previewTarget = Target;

        if (Target == ID.MiningBox) BlockPreview.Apply(_blockObj, ID.OverlayBlock, 1.04f);
        else if (held.Type == ItemType.Structure) BlockPreview.Apply(_blockObj, ID.OverlayBlock, 1f);
        else if (held.Type == ItemType.Block) BlockPreview.Apply(_blockObj, Target, 1f);
    }
    
    public static void Update()
    {
        if (GUIStorage.HoveringSlot)
        {
            _blockObj.SetActive(false);
            return;
        }

        if (Control.Inst.terraform.KeyDown())
            _overlayEnabled = !_overlayEnabled;

        Item held = Inventory.CurrentItemData;
        Target = Aim.IsMiningTool() ? ID.MiningBox
            : held != null && held.Type is ItemType.Block or ItemType.Structure ? held.ID
            : ID.Null;
        ConfigurePreview(held);

        Vector3Int cell = Aim.Cell();
        bool aimVisible = Target != ID.MiningBox || _overlayEnabled;
        bool cellValid = Target != ID.Null && Control.MouseLayer != -1 &&
                         World.IsInWorldBounds(cell) && Scene.InPlayerBlockRange(cell, Reach) &&
                         IsCellPlaceable(cell);

        _blockObj.SetActive(aimVisible && cellValid);
        if (aimVisible && cellValid)
            Position(_blockObj, cell);

        bool overMap = Helper.isLayer(Control.MouseLayer, Main.IndexMap) &&
                       Main.PlayerInfo.Machine.IsCurrentState<DefaultState>();
        if (aimVisible && cellValid && overMap && !PendingBlocks.Contains(cell) &&
            Control.Inst.ActionPrimary.Key())
            Place(held, cell);
    }

    private static void Place(Item held, Vector3Int cell)
    {
        if (Target == ID.MiningBox)
        {
            if (Main.CreativeMode) World.SetBlock(cell);
            else Entity.Spawn(ID.MiningBox, cell);
            return;
        }

        Main.PlayerInfo.Machine.SetState<MobAttackSwing>();
        Audio.PlaySFX(held.Sfx);

        if (held.Type == ItemType.Structure)
        {
            Entity.Spawn(Target, cell);
            Tutorial.OnPlaced(Target);
            RemoveHeldItem();
            return;
        }

        if (Main.CreativeMode)
        {
            Main.PlayerInfo.Storage.CreateAndAddItem(Target);
            World.SetBlock(cell, Block.ConvertID(Target));
            return;
        }

        Tutorial.OnBlockPlaced();
        World.SetBlock(cell, Block.ConvertID(Target));
        RemoveHeldItem();
    }

    private static void Position(GameObject obj, Vector3Int cell)
    {
        obj.transform.position = Vector3.Lerp(obj.transform.position,
            cell + new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
    }
  
    private static void RemoveHeldItem()
    {
        Inventory.CurrentItem.Stack--;
        if (Inventory.CurrentItem.Stack <= 0) Inventory.CurrentItem.clear();
        Inventory.RefreshInventory();
    }

    private static bool IsCellPlaceable(Vector3Int cell)
    {
        return Target == ID.MiningBox
            ? Block.GetBlock(World.GetBlock(cell)) != null
            : World.GetBlock(cell) == 0;
    }
}
