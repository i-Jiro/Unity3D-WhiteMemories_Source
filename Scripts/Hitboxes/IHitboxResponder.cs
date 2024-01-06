using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitboxResponder
{
    void CollisionedWith(Collider other, Vector3 direction = default);

}
