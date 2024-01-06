using System;
using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the UI for the item description box.
/// </summary>

public class ItemDescriptonBox : MonoBehaviour
{
    public enum STATE
    {
        SHOW_DESCRIPTION,
        ITEM_INTERACT
    }
    
    private STATE _currentState;
    public STATE CurrentState 
    {
        get => _currentState;
        set
        {
            _currentState = value;
            switch (value)
            {
                case STATE.SHOW_DESCRIPTION:
                    _interactContainer.SetActive(false);
                    _descriptionText.gameObject.SetActive(true);
                    break;
                case STATE.ITEM_INTERACT:
                    _interactContainer.SetActive(true);
                    _descriptionText.gameObject.SetActive(false);
                    break;
                default:
                    Debug.LogError("Invalid state");
                    break;
            }   
        }
    }
    
    [Required]
    [SerializeField] private InventoryUIController _inventoryUIController;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] public GameObject _interactContainer;
    [Header("Button References")]
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _discardButton;
    [SerializeField] private Button _cancelButton;

    [SerializeField] private Button _SlotOne;
    [SerializeField] private Button _SlotTwo;

    private Button[] _buttons;

    private void Awake()
    {
        CurrentState = STATE.SHOW_DESCRIPTION;
        _equipButton.onClick.AddListener(()=>
        {
            SlotItem();
        });
        _useButton.onClick.AddListener(()=>
        {
            _inventoryUIController.UseSelectedItem();
            ExitInteract();
        });
        _discardButton.onClick.AddListener(()=>
        {
            _inventoryUIController.DiscardItem();
            ExitInteract();
        });
        _cancelButton.onClick.AddListener(()=>
        {
            _inventoryUIController.CancelInteract();
            ExitInteract();
        });
        _SlotOne.onClick.AddListener(()=>
        {
            _inventoryUIController.EquipItemToQuickSlot(0);
            ExitInteract();
        });
        _SlotTwo.onClick.AddListener(()=>
        {
            _inventoryUIController.EquipItemToQuickSlot(1);
            ExitInteract();
        });
        
        _buttons = new Button[] {_equipButton, _useButton, _discardButton, _SlotOne, _SlotTwo, _cancelButton};
    }
    
    /// <summary>
    /// Dynamically changes the navigation of the buttons based on which ones are active.
    /// </summary>
    private void UpdateButtonNavigation()
    {
        var activeButtons = new List<Button>();
        foreach (var button in _buttons)
        {
            if (button.gameObject.activeSelf)
            {
                activeButtons.Add(button);
            }
        }
        
        //Change the navigation of the buttons vertically
        for(int i = 0; i < activeButtons.Count; i++)
        {
            Navigation nav = activeButtons[i].navigation;
            if (i == 0)
            {
                nav.selectOnUp = activeButtons[activeButtons.Count-1];
            }
            else
            {
                nav.selectOnUp = activeButtons[i-1];
            }
            
            if (i == activeButtons.Count-1)
            {
                nav.selectOnDown = activeButtons[0];
            }
            else
            {
                nav.selectOnDown = activeButtons[i+1];
            }

            activeButtons[i].navigation = nav;
        }
    }

    /// <summary>
    /// Reveals the item interact buttons based on item type and data.
    /// </summary>
    /// <param name="item"></param>
    public void EnterInteract(Item item)
    {
        _equipButton.gameObject.SetActive(item is ConsumableItem);
        _useButton.gameObject.SetActive(item is ConsumableItem);
        _discardButton.gameObject.SetActive(item.Discardable);
        _SlotOne.gameObject.SetActive(false);
        _SlotTwo.gameObject.SetActive(false);
        UpdateButtonNavigation();
        CurrentState = STATE.ITEM_INTERACT;
        //Select the first active button.
        for(int i = 0; i < _buttons.Length; i++)
        {
            if(_buttons[i].gameObject.activeSelf == false) continue;
            _buttons[i].Select();
            break;
        }
    }
    
    /// <summary>
    /// Reveal the quick slot buttons.
    /// </summary>
    public void SlotItem()
    {
        _equipButton.gameObject.SetActive(false);
        _useButton.gameObject.SetActive(false);
        _discardButton.gameObject.SetActive(false);
        _SlotOne.gameObject.SetActive(true);
        _SlotTwo.gameObject.SetActive(true);
        UpdateButtonNavigation();
        _SlotOne.Select();
    }
    
    public void ExitInteract()
    {
        CurrentState = STATE.SHOW_DESCRIPTION;
    }

    public void SetDescription(string text)
    {
        _descriptionText.text = text;
    }

    private void OnDestroy()
    {
        _equipButton.onClick.RemoveAllListeners();
        _useButton.onClick.RemoveAllListeners();
        _discardButton.onClick.RemoveAllListeners();
    }
}
