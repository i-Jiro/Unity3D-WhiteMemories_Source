using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class WithInDistance : Conditional
{
    public SharedGameObject Target;
    public float DistanceRange = 1f;
    public bool IgnoreHeight = false;
    public bool DebugRay = false;

    public override TaskStatus OnUpdate()
    {
        if (Target == null)
        {
            Debug.LogWarning($"{gameObject.name}: target object was null." );
            return TaskStatus.Failure;
        }

        Vector3 fromTo = Target.Value.transform.position - transform.position;
        if(IgnoreHeight)
            fromTo.y = 0;
        if (DebugRay)
        {
            Debug.DrawRay(transform.position, fromTo, Color.red);       
        }
        float distance = fromTo.magnitude;
        return distance < DistanceRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}
