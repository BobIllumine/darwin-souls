// using System.Collections;
// using System.Collections.Generic;
// using System.Reflection;
// using UnityEngine;

// [RequireComponent(typeof(BossState))]
// [RequireComponent(typeof(BossAnimResolver))]
// [RequireComponent(typeof(BossMovementController))]
// public class BossActionController : BaseActionController
// {
//     void Start()
//     {
//         state = GetComponent<BossState>();
//         animResolver = GetComponent<BossAnimResolver>();
//         movementController = GetComponent<BossMovementController>();
//         isActionable = true;
//         canAttack = true;
//         canCast = true;

//         actionSpace = new Dictionary<string, Action>() {
//             ["defaultAttack"] = gameObject.GetComponentInChildren<DefaultAttack>().Initialize(gameObject),
//         };
//     }

//     public override void Do(string name)
//     {
//         if(!isActionable || (!canCast && name != "defaultAttack") || (!canAttack && name == "defaultAttack"))
//             return;
//         try
//         {
//             movementController.isMovable = false;
//             activeAction = actionSpace[name];
//             activeAction.Fire(state.CR);
//         }
//         catch(KeyNotFoundException e)
//         {
//             print(e);
//             movementController.isMovable = true;
//             Debug.Log("bad luck kiddo");
//             return;    
//         }
//         movementController.isMovable = true;
//     }
// }
