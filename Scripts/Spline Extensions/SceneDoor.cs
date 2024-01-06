using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SceneDoor : MonoBehaviour
{
    [Header("Scene to transition to")]
    [SerializeField] private string _sceneName;
    [SerializeField] private string _exitName = "None";
    public bool TransitionOnEnter = false;
    
    [Header("Spline")]
    [Required]
    [SerializeField] private Spline _spline;
    [SerializeField] [Range(0, 1)] private float _pointOnSpline;
    
    [Header("Trigger Collider")]
    [Required]
    [SerializeField] private BoxCollider _collider;
    
    [Header("Properties")]
    public bool IsLocked = false;
    [SerializeField] private Color _gizmoColor = new Color(178, 0, 191, 0.75f);
    
    [Header("Events")]
    [SerializeField] private UnityEvent _onPlayerEnter;
    [SerializeField] private UnityEvent _onPlayerExit;
    [SerializeField] private UnityEvent _onPlayerEnterLocked;
    
    private bool _playerInside;
    private bool _hasActivated;

    private void OnEnable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.InteractPressed += OnInteractPressed;
    }

    private void OnDisable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.InteractPressed -= OnInteractPressed;
    }
    
    private void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("LastExitPoint")) return;
        if (PlayerPrefs.GetString("LastExitPoint") == _exitName)
        {
            PlayerBaseController.Instance.transform.position = transform.position;
            var splineWalker = PlayerBaseController.Instance.GetComponent<SplineWalker>();
            splineWalker.SetSpline(_spline);
        }
    }

    private void OnValidate()
    {
        if(_spline == null) return;
        if(_spline.AnchorPoints.Count < 3) return;
        var t = _pointOnSpline * ((float)_spline.AnchorPoints.Count - 2);
        var point = _spline.CalculatePoint(t);
        transform.position = point;
    }

    public void SetLockState(bool value)
    {
        IsLocked = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLocked)
        {
            _onPlayerEnterLocked?.Invoke();
            return;
        }
        if (!other.CompareTag("Player")) return;
        if (TransitionOnEnter && !_hasActivated)
        {
            _hasActivated = true;
            PlayerPrefs.SetString("LastExitPoint", _exitName);
            SceneLoader.LoadScene(_sceneName);
            return;
        }
        _playerInside = true;
        _onPlayerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLocked) return;
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        _onPlayerExit?.Invoke();
    }

    private void OnInteractPressed()
    {
        if (!_playerInside) return;
        if (_hasActivated) return; //Prevent multiple calls, will reset on scene change.
        PlayerPrefs.SetString("LastExitPoint", _exitName);
        SceneLoader.LoadScene(_sceneName);
        _hasActivated = true;
    }
    
    private void OnDrawGizmos()
    {
        if (!_collider) return;
        Gizmos.color = _gizmoColor;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(transform.InverseTransformPoint(_collider.bounds.center), new Vector3(_collider.size.x, _collider.size.y, _collider.size.z));
    }
}
