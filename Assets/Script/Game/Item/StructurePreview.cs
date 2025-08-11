using UnityEngine;

public class StructurePreviewMachine : BasicMachine
{   
        public override void OnStart()
        { 
                AddModule(new SpriteOrbitModule(transform));
                AddState(new DefaultState());
                AddState(new StructurePreviewState());
                transform.localScale *= 2;
        }

        public override void OnUpdate()
        {
                if (StructureRecipe.Target != null)
                        SetState<StructurePreviewState>();
        }
}

public class StructurePreviewState : State
{
        private static SpriteRenderer _spriteRenderer;
        public override void Initialize()
        {
                _spriteRenderer = Machine.GetComponent<SpriteRenderer>();
                _spriteRenderer.enabled = false; 
        }

        public override void OnEnterState()
        { 
                _spriteRenderer.sprite = Cache.LoadSprite("sprite/" + StructureRecipe.Target.StringID); 
                Machine.transform.position = Game.Player.transform.position;
        }

        public override void OnUpdateState()
        {
                if (StructureRecipe.Target == null)
                        Machine.SetState<DefaultState>();
                
                Vector3Int position = Vector3Int.FloorToInt(Control.MousePosition - Control.MouseDirection * 0.1f);
                
                if (!Control.MouseTarget && Scene.InPlayerBlockRange(position, 6) &&
                    World.GetBlock(position + Vector3Int.down) != 0)
                { 
                        _spriteRenderer.enabled = true;
                        Machine.transform.position = Vector3.Lerp(Machine.transform.position, 
                               position + new Vector3(0.5f, 0, 0.5f), Time.deltaTime * 10);
                        if (!GUIMain.IsHover && Control.Inst.ActionPrimary.KeyDown())
                        { 
                                StructureRecipe.Build(position);
                                StructureRecipe.Target = null;
                        }
                }
                else
                {
                        _spriteRenderer.enabled = false;
                }
        }

        public override void OnExitState()
        {
                _spriteRenderer.enabled = false;
                StructureRecipe.Target = null;
        }
}