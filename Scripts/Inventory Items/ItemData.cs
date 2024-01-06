using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.GameItems
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item Data")]
    public class ItemData : ScriptableObject, ISerializeReferenceByCustomGuid
    {
        [Title("Item Data")]
        [ValidateInput("ValidateID", "ID is invalid")]
        [LabelText("Unique ID")]
        [SerializeField] private string _guid;
        public string Guid => _guid;
        [field: SerializeField] public string DisplayName { get; private set; }
        [MultiLineProperty(3)]
        [SerializeField]private string _description;
        public string Description => _description;
        [PreviewField]
        [SerializeField] private Sprite _icon;
        public Sprite Icon => _icon;
        [SerializeField] private bool _discardable = true;
        public bool Discardable => _discardable;
        [field: SerializeField] public bool Stackable { get; private set; } = false;
        [field: SerializeField] public int MaxStack { get; private set; }= 1;

        
        /// <summary>
        /// Validation check for GUID for Odin's Validator. Checks if the ID is unique and is not empty.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private bool ValidateID(string id, ref string errorMessage)
        {
            if (id == String.Empty)
            {
                errorMessage = "ID cannot be empty!";
                return false;
            }
            List<ItemData> allItems = new List<ItemData>();
            allItems.AddRange(Resources.LoadAll<ItemData>("Items"));
            foreach (var item in allItems)
            {
                if(item == this) continue;
                if (item.Guid == id)
                {
                    errorMessage = "ID must be unique! Conflicting with Item: \n" +
                                   item.name;
                    return false;
                }
            }
            return true;
        }
    }
}
