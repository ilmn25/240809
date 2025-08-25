//  
// using UnityEngine;
//
// public class ItemPhysicModule : EntityModule
// {
//     private ItemInfo _info;
//
//     public ItemInfo Info
//     {
//         get
//         {
//             if (_info == null)
//             {
//                 _info = (ItemInfo)EntityMachine.Info;
//             }
//             return _info;
//         }
//     }
//     
//     private float _deltaTime;
//
//     private const float Gravity = 35;
//     private const float BounceFactor = 0.3f;
//     private const float CollisionRange = 0.3f;
//
//     private static readonly Collider[] TempCollisionArray = new Collider[1];
//     private static int _collisionCount;
//     public override void Update()
//     { 
//         if (!Info.IsInRenderRange)  return;
//         _deltaTime = Helper.GetDeltaTime();
//
//         if (!IsMovable(Machine.transform.position))
//         {
//             Machine.transform.position += new Vector3(0, 5, 0) * _deltaTime;
//             return;
//         }
//
//         Info.Velocity += Gravity * _deltaTime * Vector3.down;
//         Info.Velocity.y = Mathf.Max(Info.Velocity.y, -Gravity); 
//         Vector3 newPosition = Machine.transform.position + Info.Velocity * _deltaTime;
//         
//         if (IsMovable(newPosition))
//         {
//             Machine.transform.position = newPosition;
//         }
//         else
//         { 
//             Info.Velocity = -Info.Velocity * BounceFactor;
//         }
//
//         return;
//
//         bool IsMovable(Vector3 pos)
//         {  
//             TempCollisionArray[0] = null;
//             _collisionCount = Physics.OverlapSphereNonAlloc(pos + new Vector3(0,0.2f,0), CollisionRange, TempCollisionArray, Game.MaskStatic);
//
//             return !(_collisionCount > 0);
//         } 
//     }
//  
//       
// }
