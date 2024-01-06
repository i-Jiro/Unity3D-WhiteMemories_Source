using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class FacingTarget : Conditional
{
    public SharedGameObject Target;
    public SharedGameObject Self;
    public float AngleThreshold = 45f;
    public override TaskStatus OnUpdate()
    {
        if(Target == null || Self == null)
        {
            Debug.LogWarning($"{gameObject.name}: target or self object was null." );
            return TaskStatus.Failure;
        }
        
        Vector3 fromTo = Target.Value.transform.position - Self.Value.transform.position;
        fromTo.y = 0;
        var forward = Self.Value.transform.forward;
        forward.y = 0;
        float angle = Vector3.Angle(fromTo, forward);
        return angle < AngleThreshold ? TaskStatus.Success : TaskStatus.Failure;
    }
}
