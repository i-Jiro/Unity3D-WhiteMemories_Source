using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : Selectable, ISubmitHandler, IPointerClickHandler
{
    [field: SerializeField] public InventoryUIController InventoryUI { get; private set; }
    public Image Background;
    public Image ItemBackground;
    public Image ItemIcon;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemCount;
    public Item item;
    [Tooltip("{0} = Dyanmic value")]
    [SerializeField] private string _quantityFormat = "x{0}";
    [SerializeField] private string _nameFormat = "// {0}";
    [Header("Sprite References")]
    [SerializeField] private Sprite _defaultSlotBackground;
    [SerializeField] private Sprite _selectedSlotBackground;
    [SerializeField] private Sprite _defaultItemBackground;
    [SerializeField] private Sprite _selectedItemBackground;

    public override void OnSelect(BaseEventData eventData)
    {
        if(AudioManager.Instance != null)
            AudioManager.PlayOneShot(AudioManager.Instance.UIEvents.UIMove);
        InventoryUI.SetDescriptionText(item.Description);
        Background.sprite = _selectedSlotBackground;
        ItemBackground.sprite = _selectedItemBackground;
        //base.OnSelect(eventData);
    }
    
    public override void OnDeselect(BaseEventData eventData)
    {
        InventoryUI.SetDescriptionText(string.Empty);
        Background.sprite = _defaultSlotBackground;
        ItemBackground.sprite = _defaultItemBackground;
        //base.OnDeselect(eventData);
    }
    
    //Called when then UI slot is clicked by the pointer.
    public void OnPointerClick(PointerEventData eventData)
    {
        //Ignore if the click is not a left click.
        if (eventData.button != PointerEventData.InputButton.Left) return;
        Activate();
    }

    //Called when UI slot is selected and the submit button is pressed.
    public void OnSubmit(BaseEventData eventData)
    {
        DoStateTransition(Selectable.SelectionState.Pressed, false);
        StartCoroutine(OnFinishSubmit());
        Activate();
    }
    
    private void Activate()
    {
        InventoryUI.Interact(this);
    }
    
    public void RegisterInventoryUI(InventoryUIController inventoryUI)
    {
        InventoryUI = inventoryUI;
    }

    /// <summary>
    /// Loads the item data into the UI slot.
    /// </summary>
    /// <param name="item"></param>
    public void UpdateUI(Item item)
    {
        this.item = item;
        ItemIcon.sprite = item.Icon;
        ItemName.text = string.Format(_nameFormat, item.DisplayName);
        if(item.Stackable && item.CurrentStack > 1)
            ItemCount.text = string.Format(_quantityFormat, item.CurrentStack);
        else
            ItemCount.text = string.Empty;
    }

    /// <summary>
    /// Resets the UI slot to its default state.
    /// </summary>
    public void ResetUI()
    {
        item = null;
        ItemIcon.sprite = null;
        ItemName.text = string.Empty;
        ItemCount.text = string.Empty;
    }

    //Resets the UI slot to its default state after submit.
    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        DoStateTransition(currentSelectionState, false);
    }

}
