using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UIMissionData
{
    public string title;
    public string description;

    public float pointReward;

    public bool selected;
    public bool completed;
}

public class UIMission : MonoBehaviour
{
    [SerializeField] private UIMissionData data;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnClick()
    {
        data.selected = !data.selected;

        UpdateSelectedStatus();
    }

    public UIMissionData GetMissionData()
    {
        return data;
    }

    private void UpdateSelectedStatus()
    {

    }
}
