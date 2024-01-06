using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the visual state of the quick slot UI.
/// </summary>
public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [PreviewField]
    [SerializeField] private Sprite _defaultSprite;

    public void UpdateUI(Item item)
    {
        if (item == null)
        {
            _icon.sprite = _defaultSprite;
            _quantityText.text = string.Empty;
        }
        else
        {
            if(item.Stackable)
                _quantityText.text = item.CurrentStack.ToString();
            else
                _quantityText.text = string.Empty;
            _icon.sprite = item.DataReference.Icon;
        }
    }
}
