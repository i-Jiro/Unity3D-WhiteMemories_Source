using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Core.UI
{
    [RequireComponent(typeof(Image))]
    public class TabButton : Selectable
    {
        public TabGroup TabGroup;
        public Image Background;
        public string TabTitle;

        public UnityEvent OnTabSelected;
        public UnityEvent OnTabDeselected;

        protected override void Awake()
        {
            Background = GetComponent<Image>();
        }

        // Subscribe to the tab group on start.
        protected override void Start()
        {
            TabGroup.Subscribe(this);
        }

        // On click, set the tab button to active state.
        public override void OnPointerDown(PointerEventData eventData)
        {
            TabGroup.OnTabSelected(this);
            base.OnPointerDown(eventData);
        }

        // On enter, set the tab button to hover state.
        public override void OnPointerEnter(PointerEventData eventData)
        {
            TabGroup.OnTabEnter(this);
            base.OnPointerEnter(eventData);
        }

        // On exit, set the tab button to idle state.
        public override void OnPointerExit(PointerEventData eventData)
        {
            TabGroup.OnTabExit(this);
            base.OnPointerExit(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            TabGroup.OnTabSelected(this);
            base.OnSelect(eventData);
        }

        public void OnSelect()
        {
            OnTabSelected?.Invoke();
            if (AudioManager.Instance != null)
                AudioManager.PlayOneShot(AudioManager.Instance.UIEvents.UIMove, Vector3.zero);
        }

        public void OnDeselect()
        {
            OnTabDeselected?.Invoke();
        }
    }
}
