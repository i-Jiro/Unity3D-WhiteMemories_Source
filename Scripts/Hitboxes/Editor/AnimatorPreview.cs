using UnityEngine;
using UnityEditor;
public class AnimatorPreview : EditorWindow
{
    private GameObject selectedObject;
    private AnimationClip clipToPlay;
    private AnimationClip[] clips;
    private string[] clipTitles;
    private float time = 0.0f;
    private int selectedClipIndex = 0;
    private int lastClipIndex = 0;

    private bool pressedPlay = false;
    private readonly string[] playbackSpeeds = { "0.25", "0.5", "1.0", "1.25", "1.5", "2.0" };
    private int selectedSpeedIndex = 2;
    private float playbackSpeed;

    private bool resumePlayback = false;
    private bool applyRootMotion = true;

    [MenuItem("Tools/Animator Preview", false, 2000)]
    public static void Open()
    {
        var window = GetWindowWithRect<AnimatorPreview>(new Rect(0, 0, 300, 80));
        window.Show();
    }

    private void OnDisable()
    {
        selectedObject = null;
        clips = null;
        pressedPlay = false;
        AnimationMode.StopAnimationMode();
    }

    public void OnSelectionChange()
    {
        if (Selection.activeGameObject.GetComponent<Animator>())
        {
            selectedObject = Selection.activeGameObject;
            clips = selectedObject.GetComponent<Animator>().runtimeAnimatorController.animationClips;
            clipTitles = new string[clips.Length];

            for (int i = 0; i < clips.Length; i++)
            {
                clipTitles[i] = clips[i].name;
            }

            if (resumePlayback)
            {
                pressedPlay = true;
            }

            AnimationMode.StartAnimationMode();
        }
        else
        {
            pressedPlay = false;
        }
        Repaint();
    }

    public void OnGUI()
    {
        if (selectedObject == null)
        {
            EditorGUILayout.HelpBox("Please select a GameObject with an Animator component", MessageType.Info);
            return;
        }

        if (clips.Length < 0)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        selectedSpeedIndex = EditorGUILayout.Popup("Playback Speed", selectedSpeedIndex, playbackSpeeds);
        if (GUILayout.Button("Play / Pause"))
        {
            pressedPlay = !pressedPlay;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        selectedClipIndex = EditorGUILayout.Popup("Animation Clips", selectedClipIndex, clipTitles);

        EditorGUILayout.BeginHorizontal();
        applyRootMotion = GUILayout.Toggle(applyRootMotion, "Apply RootMotion");

        if (applyRootMotion)
        {
            selectedObject.GetComponent<Animator>().applyRootMotion = true;
        }
        else
        {
            selectedObject.GetComponent<Animator>().applyRootMotion = false;
        }
        
        resumePlayback = GUILayout.Toggle(resumePlayback, "Resume Playback");
        EditorGUILayout.EndHorizontal();

        if (selectedClipIndex != lastClipIndex)
        {
            time = 0.0f;
            lastClipIndex = selectedClipIndex;
        }

        clipToPlay = clips[selectedClipIndex];

        if (clipToPlay != null && !pressedPlay)
        {
            float startTime = 0.0f;
            float stopTime = clipToPlay.length;
            time = EditorGUILayout.Slider(time, startTime, stopTime);
        }
        SceneView.RepaintAll();
        EditorGUILayout.EndVertical();
    }
    void Update()
    {
        if (selectedObject == null || !selectedObject.GetComponent<Animator>())
        { 
            return;
        }

        if (clipToPlay == null)
        { 
            return;
        }

        if (pressedPlay)
        {
            GetPlaybackSpeed();

            time += playbackSpeed * Time.deltaTime;
            if (time >= clipToPlay.length)
            {
                time = 0.0f;
            }
        }

        if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(selectedObject, clipToPlay, time);
            AnimationMode.EndSampling();

            SceneView.RepaintAll();
        }
    }
    private void GetPlaybackSpeed()
    {
        switch (selectedSpeedIndex)
        {
            case 0:
                playbackSpeed = 0.25f;
                break;
            case 1:
                playbackSpeed = 0.5f;
                break;
            case 2:
                playbackSpeed = 1.0f;
                break;
            case 3:
                playbackSpeed = 1.25f;
                break;
            case 4:
                playbackSpeed = 1.5f;
                break;
            case 5:
                playbackSpeed = 2.0f;
                break;
        }
    }
}
