using UnityEngine;

/// <summary>Base for ground-based mobs (animals, NPCs, enemies) that share the
/// standard movement/pathing/animation/sprite module stack. Subclasses call
/// base.OnStart() and register their own states. Hostile melee mobs opt into
/// door-bashing via UsesDoorBash.</summary>
public abstract class GroundMobMachine : MobMachine
{
    /// <summary>Whether this mob bashes doors/barricades blocking its path.</summary>
    protected virtual bool UsesDoorBash => false;

    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());
        if (UsesDoorBash)
            AddModule(new DoorBashModule());
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;
        GetModule<GroundPathingModule>().DrawGizmos();
    }
}
