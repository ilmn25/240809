using UnityEngine;

public class StructurePreviewMachine : BasicMachine
{   
        public override void OnStart()
        { 
                AddModule(new SpriteOrbitModule(transform));
                AddState(new DefaultState());
                AddState(new StructurePreviewState());
                transform.localScale = new Vector3(1, 1.414f, 1);
        }

        public override void OnUpdate()
        {
                if (StructureRecipe.Target != null)
                        SetState<StructurePreviewState>();
        }
}

public class StructurePreviewState : State
{
        private static Vector3 _targetPosition;
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
                
                if (!GUIMain.IsHover && Control.Inst.ActionSecondary.KeyDown())
                {  
                        StructureRecipe.Target = null;
                        Machine.SetState<DefaultState>(); 
                        return;
                }
                
                Vector3Int position = Vector3Int.FloorToInt(Control.MousePosition - Control.MouseDirection * 0.1f);
                
                if (!Control.MouseTarget && Scene.InPlayerBlockRange(position, 6) &&
                    World.GetBlock(position + Vector3Int.down) != 0)
                { 
                        _targetPosition = position+ new Vector3(0.5f, 0, 0.5f);
                        _spriteRenderer.enabled = true;
                        Machine.transform.position = Vector3.Lerp(Machine.transform.position, 
                                _targetPosition, Time.deltaTime * 10);
                        if (!GUIMain.IsHover && Control.Inst.ActionPrimary.KeyDown())
                        { 
                                Audio.PlaySFX("pick_up", 0.4f);
                                StructureRecipe.Build(position);
                                StructureRecipe.Target = null;
                        }
                }
                else
                {
                        Machine.transform.position = Vector3.Lerp(Machine.transform.position, 
                                _targetPosition, Time.deltaTime * 10);
                }
        }

        public override void OnExitState()
        {
                _spriteRenderer.enabled = false;
                StructureRecipe.Target = null;
        }
}