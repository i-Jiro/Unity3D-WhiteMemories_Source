using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUIManager : MonoBehaviour
{
    //Singleton
    public static DamageUIManager Instance;
    public GameObject DamagePopUpPrefab;
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("DamageUIManager already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    private void Start()
    {
        PoolManager.WarmPool(DamagePopUpPrefab, 10);
    }
    
    public void SpawnDamagePopUp(Vector3 worldPosition, float damageValue, DamagePopUp.DamageType type)
    {
        //Checks if damage popups are enabled in settings. If not, don't do anything.
        if(SettingsManager.Instance != null && !SettingsManager.Instance.DisplayDamageValues)
            return;
        var popUp = PoolManager.SpawnObject(DamagePopUpPrefab, worldPosition, Quaternion.identity);
        popUp.GetComponent<DamagePopUp>().Fire(damageValue, type);
    }
}
