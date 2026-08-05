using System.Collections;
using UnityEngine;

public abstract class CraftingMachine: StructureMachine, IActionSecondaryInteract
{ 
    /// <summary>Machines whose light shines only while they are actively crafting (furnace, campfire, smelter, ...).</summary>
    protected virtual bool GlowsWhenCrafting => false;

    public override void OnStart()
    {
        base.OnStart();
        AddState(new InCraftState());

        CraftInfo info = (CraftInfo)Info;
        if (GlowsWhenCrafting)
            SetGlow(info.IsConverting());

        IEnumerator Enumerator()
        {
            while (gameObject.activeSelf)
            {
                yield return new WaitForSeconds(3);
                if (info.IsConverting() && Helper.IsHost())
                {
                    Particle.Create(transform.position, Particles.Smoke, false);
                    Particle.Create(transform.position, Particles.Fire, false);
                }
            }
        }

        StartCoroutine(Enumerator());
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        // Keep the glow in sync with crafting on the host (authoritative) and on
        // clients (Pending is kept up to date via StorageSync).
        if (GlowsWhenCrafting && GlowLight != null && Info is StructureInfo)
            SetGlow(((CraftInfo)Info).IsConverting());
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InCraftState>();
        else 
            SetState<DefaultState>();
    }
}
public class WoodenToolbenchMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.WoodenToolbench, 500, SfxID.HitStone, SfxID.HitStone);
    }
}

public class CarpenterWorkbenchMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.CarpenterWorkbench, 500, SfxID.HitStone, SfxID.HitStone);
    }
}

public class LoomMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Loom, 500, SfxID.HitStone, SfxID.HitStone);
    }
}