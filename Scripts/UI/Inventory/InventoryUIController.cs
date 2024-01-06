using System;
using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [field: SerializeField] public PlayerData RuntimePlayerData { get; private set; }
    [SerializeField] private Core.UI.TabButton _tabButton;
    [RequiredIn(PrefabKind.PrefabAsset)]
    [SerializeField] private GameObject _inventorySlotPrefab;
    [SerializeField] private ItemDescriptonBox _itemDescriptonBox;
    [SerializeField] private List<InventorySlot> _inventorySlots;

    private Item _selectedItem;
    private InventorySlot _selectedSlot;
    
    /// <summary>
    /// Update the Inventory UI only when the active tab is the inventory tab.
    /// </summary>
    private void OnEnable()
    {
        //Update the UI when the tab is first enabled.
        UpdateUI();

        if(RuntimePlayerData == null) return;
        //Update the UI only when the inventory changes.
        RuntimePlayerData.Inventory.InventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        _itemDescriptonBox.CurrentState = ItemDescriptonBox.STATE.SHOW_DESCRIPTION;
        if(RuntimePlayerData == null) return;
        RuntimePlayerData.Inventory.InventoryChanged -= UpdateUI;
    }

    /// <summary>
    /// Updates the navigation for the tab button and inventory slots.
    /// </summary>
    private void UpdateNavigation()
    {
        var tabNavigation = _tabButton.navigation;
        //If there are slots, set the first slot as the down navigation for tab.
        if (_inventorySlots.Count > 0)
        {
            tabNavigation.selectOnDown = _inventorySlots[0];
            //Set the tab button as the up navigation for the first slot.
            var slotNavigation = _inventorySlots[0].navigation;
            slotNavigation.selectOnUp = _tabButton;
            slotNavigation.mode = Navigation.Mode.Explicit;
            //If there is more than one slot, set the up and down navigation for each slot.
            if(_inventorySlots.Count > 1)
            {
                for (int i = 0; i < _inventorySlots.Count; i++)
                {
                    var slot = _inventorySlots[i];
                    if (i == 0)
                    {
                        slotNavigation.selectOnDown = _inventorySlots[i + 1];
                    }
                    else if (i == _inventorySlots.Count - 1)
                    {
                        slotNavigation.selectOnUp = _inventorySlots[i - 1];
                    }
                    else
                    {
                        slotNavigation.selectOnUp = _inventorySlots[i - 1];
                        slotNavigation.selectOnDown = _inventorySlots[i + 1];
                    }
                    slot.navigation = slotNavigation;
                }
            }
            else
            {
                //Otherwise, just set the up navigation for the first slot.
                _inventorySlots[0].navigation = slotNavigation;
            }
        }
        else
        {
            //If there are no slots, set the down navigation for tab to null.
            tabNavigation.selectOnDown = null;
        }
        _tabButton.navigation = tabNavigation;
    }


    /// <summary>
    /// Updates the slots with from information from inventory class.
    /// </summary>
    [Button("Force Update UI"), GUIColor("Blue"), HorizontalGroup("Buttons")]
    public void UpdateUI()
    {
#if UNITY_EDITOR
        if(Application.isPlaying == false) return;
#endif
        UpdateSlotCount();
        UpdateNavigation();
        
        //Clear all slots.
        foreach (var slot in _inventorySlots)
        {
            slot.ResetUI();
        }
        
        //Update the UI for each slot with item data.
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var slot = _inventorySlots[i];
            slot.UpdateUI(RuntimePlayerData.Inventory.Items[i]);
        }
    }
    
    /// <summary>
    /// Called when a slot is selected to interact with an item.
    /// </summary>
    /// <param name="slot"></param>
    public void Interact(InventorySlot slot)
    {
        if (slot != null && slot.item == null) return;
        _selectedSlot = slot;
        _selectedItem = slot.item;
       _itemDescriptonBox.EnterInteract(_selectedItem);
    }


    //Changes the description text to match the item.
    public void SetDescriptionText(string text) => _itemDescriptonBox.SetDescription(text);


    //Callbacks for the item description box when interacting with item.
    #region CALLBACKS

    public void EquipItemToQuickSlot(int index)
    {
        if(_selectedItem == null) return;
        RuntimePlayerData.Inventory.SetQuickSlot(index,_selectedItem);
        _selectedSlot.Select();
        _selectedItem = null;
        _selectedSlot = null;
    }
    
    public void DiscardItem()
    {
        if(_selectedItem == null) return;
        RuntimePlayerData.Inventory.RemoveItem(_selectedItem.DataReference);
        _selectedItem = null;
        _selectedSlot = null;
        if (_inventorySlots.Count > 0)
        {
            _inventorySlots[0].Select();
        }
        else
        {
            _tabButton.Select();
        }
    }

    public void UseSelectedItem()
    {
        if(_selectedItem == null) return;
        //Cast the item to a consumable item
        ConsumableItem item = (ConsumableItem) _selectedItem;
        
        //Check if the item is of type recovery item and then use it
        if (item is RecoveryItem)
        {
            RecoveryItem recoveryItem = (RecoveryItem) item;
            recoveryItem.Use();
        }
        else
            item.Use();
        //Remove the item from the inventory if it is not reusable
        if(!item.Reusable)
        {
            RuntimePlayerData.Inventory.RemoveItem(_selectedItem.DataReference);
        }
        _selectedItem = null;
        _selectedSlot = null;
        if (_inventorySlots.Count > 0)
        {
            _inventorySlots[0].Select();
        }
        else
        {
            _tabButton.Select();
        }
    }

    public void CancelInteract()
    {
        if(_selectedItem == null) return;
        _selectedSlot.Select();
        _selectedItem = null;
        _selectedSlot = null;
    }
    #endregion

    /// <summary>
    /// Adjust the numbers of inventory slots to match the number of items in the inventory.
    /// </summary>
    private void UpdateSlotCount()
    {
        var totalItems = RuntimePlayerData.Inventory.Items.Count;
        var totalSlots = _inventorySlots.Count;
        
        //If there are more items than slots, add more slots.
        if (totalItems >= totalSlots)
        {
            var difference = totalItems - totalSlots;
            for (int i = 0; i < difference; i++)
            {
                //Create a new UI slot as child of the controller.
                var newSlot = Instantiate(_inventorySlotPrefab, transform).GetComponent<InventorySlot>();
                newSlot.RegisterInventoryUI(this);
                _inventorySlots.Add(newSlot);
            }
        }
        //If there are more slots than items, remove slots.
        else if (totalItems < totalSlots)
        {
            var difference = totalSlots - totalItems;
            for (int i = 0; i < difference; i++)
            {
                var slot = _inventorySlots[_inventorySlots.Count - 1];
                _inventorySlots.Remove(slot);
                Destroy(slot.gameObject);
            }
        }
    }
}
