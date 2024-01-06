using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public enum ColliderState
{
    Closed,
    Open,
    Colliding,
    Parryable
}

public class HitboxController : MonoBehaviour
{
    [SerializeField] private int _guid = 0;
    public int GUID => _guid;
    
    public Vector3 hitBoxSize = Vector3.one * 0.25f;
    [FormerlySerializedAs("mask")] public LayerMask hurtBoxMask;
    public Color inactiveColor = new Color(255, 255, 255, 0.75f);
    public Color collisionOpenColor = new Color(44, 144, 53, 0.75f);
    public Color collidingColor = new Color(180, 28, 34, 0.75f);
    public Color parryableColor = Color.yellow;

    public bool alwaysVisible = false;
    public bool showWireFrame = false;

    private ColliderState _state = ColliderState.Closed;
    private IHitboxResponder _responder = null;
    private IParryable parriedObject = null;

    private List<Collider> _collidersHit = new List<Collider>();

    #region Draw Gizmo
    private void OnDrawGizmosSelected()
    {
        if (alwaysVisible)
        {
            return;
        }

        DrawHitbox();
    }
    private void OnDrawGizmos()
    {
        if (!alwaysVisible)
        {
            return;
        }

        DrawHitbox();
    }

    private void Awake()
    {
        parriedObject = transform.root.GetComponent<IParryable>();
    }

    private void DrawHitbox()
    {
        CheckGizmoColor();
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, hitBoxSize);

        if (showWireFrame)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
        }
    }

    private void CheckGizmoColor()
    {
        switch (_state)
        {
            case ColliderState.Closed:
                Gizmos.color = inactiveColor;
                break;
            case ColliderState.Open:
                Gizmos.color = collisionOpenColor;
                break;
            case ColliderState.Parryable:
                Gizmos.color = parryableColor;
                break;
            case ColliderState.Colliding:
                Gizmos.color = collidingColor;
                break;
        }
    }
    #endregion

    //Called in an update method for every frame.
    //A boxcast is used to check for collisions based on hitbox dimension.
    public void Tick()
    {
        if (_state == ColliderState.Closed) { return; }
        Collider[] colliders = Physics.OverlapBox(transform.position, hitBoxSize/2, transform.rotation, hurtBoxMask);
        bool collidedWithSelf = false;

        foreach (var collider in colliders)
        {
            //Prevents the same collider to get hit more than once.
            if (_collidersHit.Contains(collider))
                continue;
            //Skip if the collider doesn't have a hurtbox controller.
            if(!collider.TryGetComponent<HurtboxController>(out var hurtBox))
                continue;
            //Skip if the hurtbox is owned by the same object as the hitbox.
            if (hurtBox.Owner == transform.root.gameObject)
            {
                collidedWithSelf = true;
                continue;
            }

            _collidersHit?.Add(collider); //Register the collider to a list of colliders that have been hit.
            if (hurtBox.Mode == HurtboxMode.PARRY && parriedObject != null)
            {
                parriedObject.OnParried();
                continue;
            }

            var hitDirection = collider.transform.root.position - transform.root.position;
            //Report the collision to the responder/current user of the hitbox.
            _responder?.CollisionedWith(collider,hitDirection);
        }
        
        var length = collidedWithSelf ? colliders.Length - 1 : colliders.Length;
        _state = length > 0 ? ColliderState.Colliding : ColliderState.Open;
    }

    //Called by the respomder/current user of the hitbox to prepare the hitbox for collision checking.
    public void StartCheckingCollision()
    {
        _collidersHit.Clear();
        _state = ColliderState.Open;
    }
    
    //Called by the respomder/current user of the hitbox to stop the hitbox from checking for collisions.
    public void StopCheckingCollision()
    {
        _collidersHit.Clear();
        _state = ColliderState.Closed;
    }

    public void StartParryCheck()
    {
        _state = ColliderState.Parryable;
    }

    public void SetResponder(IHitboxResponder responder)
    {
        _responder = responder;
    }
}
