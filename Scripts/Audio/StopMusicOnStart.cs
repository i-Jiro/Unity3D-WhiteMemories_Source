using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMusicOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.StopBGM();
    }
}
