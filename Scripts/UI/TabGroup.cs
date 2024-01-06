using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class TabGroup : MonoBehaviour
    {
        [Header("Tab Button Sprites")] [SerializeField]
        private Sprite _tabIdle;

        [SerializeField] private Sprite _tabHover;
        [SerializeField] private Sprite _tabActive;

        [Header("Tab References")] [SerializeField]
        private TabButton _startingTab;

        [SerializeField] private TextMeshProUGUI _tabTitle;

        [InfoBox(
            "Tab buttons and content must be in the same order in hierarchy and list container to correctly display.")]
        [SerializeField]
        private List<TabButton> _tabButtons = new List<TabButton>();

        [SerializeField] private List<GameObject> _tabContent = new List<GameObject>();

        private TabButton _selectedTab;

        private void Start()
        {
            if (_startingTab != null && _tabButtons.Contains(_startingTab))
                OnTabSelected(_startingTab);
        }

        /// <summary>
        /// Adds a tab button to the tab group.
        /// </summary>
        /// <param name="tabButton"></param>
        public void Subscribe(TabButton tabButton)
        {
            if (!_tabButtons.Contains(tabButton))
            {
                _tabButtons.Add(tabButton);
            }
        }

        /// <summary>
        /// Sets the tab button to hover state on enter.
        /// </summary>
        /// <param name="tabButton"></param>
        public void OnTabEnter(TabButton tabButton)
        {
            ResetTabs();
            if (_selectedTab != null && tabButton == _selectedTab) return;
            tabButton.Background.sprite = _tabHover;
        }

        /// <summary>
        /// Sets the tab button to idle state on exit.
        /// </summary>
        /// <param name="tabButton"></param>
        public void OnTabExit(TabButton tabButton)
        {
            ResetTabs();
        }

        /// <summary>
        /// Sets the tab button to active state on click.
        /// </summary>
        /// <param name="tabButton"></param>
        public void OnTabSelected(TabButton tabButton)
        {
            if (_selectedTab != null)
            {
                _selectedTab.OnDeselect();
            }

            _selectedTab = tabButton;
            _tabTitle.text = _selectedTab.TabTitle;
            ResetTabs();
            tabButton.OnSelect();
            tabButton.Background.sprite = _tabActive;

            //Swap tab content based on the tab button index in the hierarchy.
            int index = tabButton.transform.GetSiblingIndex();
            for (int i = 0; i < _tabContent.Count; i++)
            {
                _tabContent[i].SetActive(index == i);
            }
        }

        /// <summary>
        /// Resets all tabs to idle state.
        /// </summary>
        public void ResetTabs()
        {
            foreach (var tabButton in _tabButtons)
            {
                if (_selectedTab != null && tabButton == _selectedTab) continue;
                tabButton.Background.sprite = _tabIdle;
            }
        }

        public TabButton GetPreviousTab()
        {
            var index = _selectedTab.transform.GetSiblingIndex();
            var next = index - 1;
            if (next < 0)
                next = _tabButtons.Count - 1;
            return _tabButtons[next];
        }

        public TabButton GetNextTab()
        {
            var index = _selectedTab.transform.GetSiblingIndex();
            var next = index + 1;
            if (next > _tabButtons.Count - 1)
            {
                next = 0;
            }

            return _tabButtons[next];
        }
    }
}
