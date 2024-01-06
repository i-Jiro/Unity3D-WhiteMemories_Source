using System;
using UnityEngine;

#if UNITY_EDITOR
[Serializable]
public class DENodeData
{
    public string GUID;
    public string Title;
    [TextArea(5, 20)] public string Text;
    public bool IsEntryPoint;
    public DEGroup Group;
    public Vector2 Position;
}
#endif
