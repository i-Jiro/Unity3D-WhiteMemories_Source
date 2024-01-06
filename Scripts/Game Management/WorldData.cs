using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public List<int> PermaDeadEnemyKeys = new List<int>();
    public List<string> PermaDeadEnemyName = new List<string>();
    
    public List<int> GlobalSwitchKeys = new List<int>();
    public List<GlobalSwitchData> GlobalSwitchData = new List<GlobalSwitchData>();
    public List<bool> GlobalSwitchValue = new List<bool>();
}
