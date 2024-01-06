using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public string SceneName;
    public void LoadSceneByName()
    {
        if(SceneLoader.Instance != null)
            SceneLoader.LoadScene(SceneName);
        else
        {
            Debug.LogWarning("SceneLoader is not present in the scene.");
        }
    }
}
