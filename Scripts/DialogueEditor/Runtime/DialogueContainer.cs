using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<DENodeData> NodeData = new();
    public List <DEActionNodeData> ActionData = new();
    public List<DEGroupData> GroupData = new();
    public List<DENodeConnectionData> ConnectionData = new();
}
#endif
