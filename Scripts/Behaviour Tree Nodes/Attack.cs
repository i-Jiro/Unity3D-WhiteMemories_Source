using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public class Attack : EnemyAction
    {
        public string AttackName;
        public override void OnStart()
        {
            base.OnStart();
            _enemyController.Attack(AttackName);
        }
        
        public override TaskStatus OnUpdate()
        {
            if(_enemyController.CurrentAttack == null)
            {
                Debug.LogWarning($"{transform.gameObject.name}: Unable to find an attack named {AttackName}.");
                return TaskStatus.Failure;
            }
            return _enemyController.CurrentAttack.IsActive ? TaskStatus.Running : TaskStatus.Success;
        }
    }
}
