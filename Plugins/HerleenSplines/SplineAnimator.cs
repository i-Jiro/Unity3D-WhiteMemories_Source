using UnityEngine;
using UnityEngine.Events;

public class SplineAnimator : MonoBehaviour
{
    [SerializeField] private Spline spline;
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private Direction currentDirection = Direction.Forwards;
    [SerializeField] private AnimatorType currentAnimator = AnimatorType.Continuous;
    [SerializeField] private KeyCode rightInput = KeyCode.D;
    [SerializeField] private KeyCode leftInput = KeyCode.A;
    private float progress = 1f;
    public enum AnimatorType { Keyboard, Continuous } 
    public enum Direction { Forwards, Backwards }

    public AnimatorType CurrentAnimator { get { return currentAnimator; } set { currentAnimator = value; } }
    public Direction CurrentDirection { get { return currentDirection; } set { currentDirection = value; } }
    public float Progress { get { return progress; } set { progress = value; } }
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    public UnityEvent OnStart;
    public UnityEvent OnUpdate;
    public UnityEvent OnProgressCompleted;
    public UnityEvent OnLoopCompleted;
    public UnityEvent OnRemoved;
    public UnityEvent OnCollisionEntered;
    public UnityEvent OnCollisionExited;
    public UnityEvent OnTriggerEntered;
    public UnityEvent OnTriggerExited;

    private void Awake()
    {
        if (spline == null)
        {
            spline = GameObject.Find("Spline").GetComponent<Spline>();
        }
    }

    private void Start()
    {
        OnStart.Invoke();
    }

    private void Update()
    {
        OnUpdate.Invoke();
        AnimateInput();
        SplineAnimate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEntered.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExited.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExited.Invoke();
    }
    private void OnDestroy()
    {
        OnRemoved.Invoke();
    }

    private void OnDisable()
    {
        OnRemoved.Invoke();
    }

    private void AnimateInput()
    {
        switch (currentAnimator)
        {
            case AnimatorType.Keyboard:
                if (Input.GetKey(leftInput))
                {
                    if (currentDirection == Direction.Forwards)
                    {
                        progress -= moveSpeed * Time.deltaTime;
                    }
                    else
                    {
                        progress += moveSpeed * Time.deltaTime;
                    }
                }

                if (Input.GetKey(rightInput))
                {

                    if (currentDirection == Direction.Forwards)
                    {
                        progress += moveSpeed * Time.deltaTime;
                    }
                    else
                    {
                        progress -= moveSpeed * Time.deltaTime;
                    }

                }
                break;
            case AnimatorType.Continuous:
                if (currentDirection == Direction.Forwards)
                {
                    progress += moveSpeed * Time.deltaTime;
                }
                else
                {
                    progress -= moveSpeed * Time.deltaTime;
                }
                break;
        }
    }
    private void SplineAnimate()
    {
        if (progress >= ((float)spline.NumPoints - 2))
        {
            if (spline.Closed)
            {
                progress -= (float)spline.NumPoints - 1;
                OnLoopCompleted.Invoke();
            }
            else
            {
                progress = (float)spline.NumPoints - 2;
                OnProgressCompleted.Invoke();
            }
        }

        if (progress < 1f)
        {
            if (spline.Closed)
            {
                switch (currentAnimator) 
                {
                    case AnimatorType.Keyboard:
                        if (Input.GetKey(leftInput))
                        {
                            progress += (float)spline.NumPoints - 3;
                        }

                        else if (Input.GetKey(rightInput))
                        {
                            progress += (float)spline.NumPoints;
                        }
                        break;
                    case AnimatorType.Continuous:
                        if (currentDirection == Direction.Forwards)
                        {
                            progress += (float)spline.NumPoints;
                        }
                        else
                        {
                            progress += (float)spline.NumPoints - 3;
                        }
                        break;
                }
                OnLoopCompleted.Invoke();
            }
            else
            {
                progress = 1f;
            }
        }

        Vector3 animatorPosition = spline.CalculatePoint(progress);
        transform.position = animatorPosition + spline.transform.position;
    }
}
