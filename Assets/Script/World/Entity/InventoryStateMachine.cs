using UnityEngine;

public class InventoryStateMachine : StateMachine
{
        public static InventoryStateMachine Instance { get; private set; }  
        public override void OnAwake()
        {
                Instance = this;
                AddState(new ItemEmpty(), true);
                AddState(new ItemBlock());
        }
        public void HandleItemUpdate()
        {
                if (PlayerInventoryStatic.CurrentItem == null)
                {
                        SetState<ItemEmpty>();
                        return;
                }
                switch (ItemLoadStatic.GetItem(PlayerInventoryStatic.CurrentItem.StringID).Type)
                {
                        case ItemType.Block:
                                SetState<ItemBlock>();
                                break;
                        case ItemType.Tool:
                                break;
                }
        }
}

public class ItemBlock : State
{
        public override void StateUpdate()
        {  
                PlayerChunkEditStatic.Instance._blockStringID = PlayerInventoryStatic.CurrentItem.StringID;
        }

}

public class ItemTool : State
{
        
}

public class ItemEmpty : State
{
        public override void OnEnterState()
        {
                if (PlayerChunkEditStatic.Instance == null) return;
                PlayerChunkEditStatic.Instance._blockStringID = null;
        }
}