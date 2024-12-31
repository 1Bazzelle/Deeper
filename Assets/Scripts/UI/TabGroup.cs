using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    private List<Tab> tabs;
    [SerializeField] private Sprite tabIdle;
    [SerializeField] private Sprite tabHover;
    [SerializeField] private Sprite tabSelected;

    [SerializeField] private List<GameObject> UiPages;

    private Tab selectedTab;
    public void Subscribe(Tab tab)
    {
        if (tabs == null) tabs = new();

        tabs.Add(tab);

        ResetTabs();
    }


    public void OnTabEnter(Tab tab)
    {
        ResetTabs();

        if(selectedTab == null || tab != selectedTab)
        tab.SetBackground(tabHover);
    }
    public void OnTabExit(Tab tab)
    {
        ResetTabs();
    }
    public void OnTabSelected(Tab tab)
    {

        if (selectedTab != null) selectedTab.Deselect();

        selectedTab = tab;
        selectedTab.Select();

        ResetTabs();

        tab.SetBackground(tabSelected);

        int index = tab.transform.GetSiblingIndex();

        for(int i = 0; i < UiPages.Count; i++)
        {
            if (i == index) UiPages[i].SetActive(true);
            else UiPages[i].SetActive(false);
        }
    }

    private void ResetTabs()
    {
        foreach(Tab tab in tabs)
        {
            if (tab == selectedTab) continue;

            tab.SetBackground(tabIdle);
        }
    }
}
