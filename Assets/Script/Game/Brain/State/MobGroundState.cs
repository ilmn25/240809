// using Newtonsoft.Json.Linq;
// using UnityEngine;
//
// public class MobGroundState : MobState
// { 
//     public override void OnEnterState()
//     {
//         AddState(new StateEmpty(), true);
//         AddState(new MobIdle());
//         AddState(new MobChase());
//         AddState(new MobRoam());
//         AddState(new MobAttackPounce());
//     }
//     
//     public override void OnUpdateState()
//     {
//         HandleInput();
//
//         if (IsCurrentState<StateEmpty>())
//         {
//             if (Status.Target)
//             {
//                 if (Vector3.Distance(Status.Target.transform.position, Machine.transform.position) < Status.AttackDistance)
//                     SetState<MobAttack>();
//                 else if (Status.PathingStatus == PathingStatus.Stuck)
//                 {
//                     SetState<MobRoam>();
//                 }
//                 else
//                 {
//                     SetState<MobChase>();
//                 }
//             }
//             else
//             { 
//                 if (Random.value > 0.5f)
//                     SetState<MobRoam>();
//                 else
//                     SetState<MobIdle>();
//             }
//         }
//     }
//     
//     void HandleInput()
//     {
//         if (Input.GetKeyDown(KeyCode.Y))
//             Status.Target = Game.Player.transform;
//         else if (Input.GetKeyDown(KeyCode.T))
//             Status.Target = null;
//         else if (Input.GetKeyDown(KeyCode.U))
//             Machine.transform.position = Game.Player.transform.position;
//     }
// }