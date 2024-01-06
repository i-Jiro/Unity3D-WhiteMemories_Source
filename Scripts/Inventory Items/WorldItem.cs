using System;
using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using UnityEngine;
using UnityEngine.Events;

public class WorldItem : MonoBehaviour
{
    public ItemData ItemData;
    public int Amount = 1;
    public UnityEvent OnPickUpEvent;
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;
    private bool _playerInRange;

    private void OnEnable()
    {
        if(PlayerInputManager.Instance != null)
            PlayerInputManager.Instance.InteractPressed += OnPickUp;
    }

    private void OnDisable()
    {
        if(PlayerInputManager.Instance != null)
            PlayerInputManager.Instance.InteractPressed -= OnPickUp;
    }

    public void OnPickUp()
    {
        if (!_playerInRange) return;
        PlayerBaseController.Instance.AddItemToInventory(ItemData, Amount);
        OnPickUpEvent?.Invoke();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke();
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnPlayerExit?.Invoke();
            _playerInRange = false;
        }
    }
}
