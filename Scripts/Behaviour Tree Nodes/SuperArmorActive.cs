using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class SuperArmorActive : EnemyConditional
{
    
    public bool Invert = false; 
   public override TaskStatus OnUpdate()
   {
       if (_enemyController.CurrentAttack == null)
       {
           return Invert ? TaskStatus.Success : TaskStatus.Failure;}
       if(_enemyController.CurrentAttack.SuperArmor)
       {
           return Invert ? TaskStatus.Failure : TaskStatus.Success;
       }
       return Invert ? TaskStatus.Success : TaskStatus.Failure;
        }
}
