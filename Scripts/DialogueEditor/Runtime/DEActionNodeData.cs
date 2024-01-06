using System;
using UnityEngine;

#if UNITY_EDITOR
[Serializable]
public class DEActionNodeData 
{
    public string GUID;
    public string Title;
    public string ActionMethod;
    public Vector2 Position;
    public DEGroup Group;
    public GameObject ActionObject;
    public string SelectedComponent;
}
#endif
