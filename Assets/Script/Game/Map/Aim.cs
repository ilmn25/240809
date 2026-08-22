using UnityEngine;

/// <summary>Resolves the cell the cursor is targeting (map block, tree, placed
/// structure). Shared by the aim highlight and placement, which both render through
/// Terraform's single preview object.</summary>
public static class Aim
{
    private const float CellOffset = 0.02f;

    /// <summary>True when the held item is a mining tool (pickaxe), which mines the
    /// target block cell directly rather than placing something.</summary>
    public static bool IsMiningTool()
    {
        Item item = Inventory.CurrentItemData;
        return item != null &&
               item.Type == ItemType.Tool &&
               item.ProjectileInfo != null &&
               item.ProjectileInfo.OperationType == OperationType.Mining;
    }

    /// <summary>The cell currently under the cursor. Shared with placement so the aim
    /// highlight matches where a mining box / block would actually go. When hovering a
    /// structure, it snaps to the cell the structure occupies.</summary>
    public static Vector3Int Cell()
    {
        if (Control.MouseTarget != null &&
            Control.MouseTarget.GetComponentInParent<EntityMachine>() is { } machine &&
            machine.Info is StructureInfo structure)
        {
            return Vector3Int.FloorToInt(structure.position);
        }

        return IsMiningTool()
            ? Vector3Int.FloorToInt(Control.MousePosition + Control.MouseDirection * CellOffset)
            : Vector3Int.FloorToInt(Control.MousePosition - Control.MouseDirection * CellOffset);
    }
}
