using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFrameCheckHandler
{
    public void OnFirstFrame();
    public void OnHitFrameStart();
    public void OnHitFrameEnd();
    public void OnLastFrame();
}
