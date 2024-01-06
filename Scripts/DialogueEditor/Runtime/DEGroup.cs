#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using UnityEngine.UIElements;
[Serializable]
public class DEGroup : Group
{
    public string GUID;
    public string Title;
    public DEGroup(Vector2 position)
    {
        SetPosition(new Rect(position, Vector2.zero));
    }

    public void Draw()
    {
        this.Q<TextField>("titleField").RegisterValueChangedCallback(text =>
        {
            Title = text.newValue;
        });
    }
}
#endif
