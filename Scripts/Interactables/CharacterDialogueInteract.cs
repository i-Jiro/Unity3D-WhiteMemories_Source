using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Yarn.Unity;

public class CharacterDialogueInteract : MonoBehaviour
{
    [SerializeField] private string _StartNode;
    [SerializeField] LookAtIK _lookAtIK;
    [Label("Look At Player On Dialogue Start")]
    [Tooltip("Automatically look at the player when dialogue starts")]
    public bool LookAtPlayerOnStart;
    public float VerticalLookOffset;
    private DialogueRunner _dialogueRunner;
    [Header("Events")]
    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;

    private Tween _lookTween;

    // Start is called before the first frame update
    void Awake()
    {
        _dialogueRunner = FindObjectOfType<DialogueRunner>();
        if(_dialogueRunner == null)
            Debug.LogError("No Dialogue Runner found in scene!");
    }
    
    public void Talk()
    {;
        if (_dialogueRunner.IsDialogueRunning) return;
        _dialogueRunner.StartDialogue(_StartNode);
        if (LookAtPlayerOnStart)
        {
            LookAtPlayer();
        }
        StartCoroutine(ConversationRoutine());
    }
    
    [YarnCommand("LookAtPlayer")]
    public void LookAtPlayer()
    {
        _lookAtIK.solver.IKPosition = GameManager.Instance.Player.transform.position + (Vector3.up * VerticalLookOffset);
        _lookTween = DOVirtual.Float(0f, 1f, 0.5f, (value) =>
        {
            _lookAtIK.solver.IKPositionWeight = value;
        });
    }
    
    [YarnCommand("ReleaseLookAtPlayer")]
    public void ReleaseLookAtPlayer()
    {
        if(_lookTween != null)
            _lookTween.Kill();
        DOVirtual.Float(1f, 0f, 0.5f, (value) =>
        {
            _lookAtIK.solver.IKPositionWeight = value;
        });
    }

    private IEnumerator ConversationRoutine()
    {
        OnDialogueStart?.Invoke();
        yield return new WaitForEndOfFrame();
        while (_dialogueRunner.IsDialogueRunning)
        {
            yield return null;
        }

        if (LookAtPlayerOnStart)
        {
            ReleaseLookAtPlayer();
        }
        OnDialogueEnd?.Invoke();
    }
}
