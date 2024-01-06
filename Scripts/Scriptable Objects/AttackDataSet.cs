using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Data Set", menuName = "Attack/Attack Data Set", order = 1)]
public class AttackDataSet : ScriptableObject
{
    [SerializeField] private List<AttackData> _attackDatas = new List<AttackData>();

    public List<AttackData> GetAttackDataList()
    {
        return _attackDatas;
    }
}
