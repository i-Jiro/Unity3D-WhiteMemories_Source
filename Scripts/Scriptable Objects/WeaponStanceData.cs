using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Player/ Weapon Stance Data")]
public class WeaponStanceData : ScriptableObject
{
    [FormerlySerializedAs("_weaponName")]
    [Tooltip("Unique name for this weapon.")]
    [SerializeField] private string _name;
    [SerializeField] private AttackDataSet _lightAttackDataSet;
    [SerializeField] private AttackDataSet _heavyAttackDataSet;
    [SerializeField] private AttackDataSet _specialAttackDataSet;
    [SerializeField]private WeaponSpecialType _specialType;
    public string Name => _name;
    public WeaponSpecialType SpecialType => _specialType;
    
    [field: SerializeField] public VoidEventChannel OnWeaponStanceEquippedChannel { get; private set; }
    [field: SerializeField] public VoidEventChannel OnWeaponStanceRemovedChannel { get; private set; }
    
    public AttackDataSet LightAttackDataSet => _lightAttackDataSet;
    public AttackDataSet HeavyAttackDataSet => _heavyAttackDataSet;
    public AttackDataSet SpecialAttackDataSet => _specialAttackDataSet;
}

public enum WeaponSpecialType
{
    None,
    Attack,
    Parry
}
