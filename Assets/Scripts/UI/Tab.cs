using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class Tab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TabGroup tabGroup;

    private Image background;

    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    void Start()
    {
        background = GetComponent<Image>();

        tabGroup.Subscribe(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }
    
    public void SetBackground(Sprite newImage)
    {
        background.sprite = newImage;
    }
    public void Select()
    {
        if(onTabSelected != null) onTabSelected.Invoke();
    }
    public void Deselect()
    {
        if(onTabDeselected != null) onTabDeselected.Invoke();
    }
}
