using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New CreatureProfile", menuName = "CreatureProfile")]
public class CreatureProfile : ScriptableObject
{
    public CreatureID id;

    [SerializeField] private string creatureName;
    [SerializeField] private string latinCreatureName;
    [SerializeField] private ZoneID zone;
    [SerializeField] private Sprite image;
    [SerializeField] private Sprite imageReal;

    public int progress { get; set; }

    public List<Picture> pictureCollection;

    [SerializeField] private int researchLevel2;
    [SerializeField] private int researchLevel3;

    [TextArea(5, 20)]
    [SerializeField] private string infoLevel1;
    [TextArea(5, 20)]
    [SerializeField] private string infoLevel2;
    [TextArea(5, 20)]
    [SerializeField] private string infoLevel3;

    public int GetProgress()
    {
        return progress;
    }

    public int GetLevel()
    {
        if (progress == 0)
        {
            return 0;
        }
        if (progress < researchLevel2)
        {
            return 1;
        }
        if (progress < researchLevel3)
        {
            return 2;
        }
        return 3;
    }
    public int GetMinValue()
    {
        switch(GetLevel())
        {
            case 1:
                return 0;
            case 2: 
                return researchLevel2;
            case 3:
                return researchLevel3;
            default:
                return 0;
        }
    }
    public int GetMaxValue()
    {
        switch (GetLevel())
        {
            case 1:
                return researchLevel2;
            default:
                return researchLevel3;
        }
    }
    public Sprite GetImage()
    {
        return image;
    }
    public ZoneID GetZone()
    {
        return zone;
    }
    public string GetName()
    {
        return creatureName;
    }
    public string GetLatinName()
    {
        return latinCreatureName;
    }
    public string GetInfo(int info)
    {
        switch(info)
        {
            case 1:
                return infoLevel1;
            case 2:
                return infoLevel2;
            case 3:
                return infoLevel3;
            default:
                Debug.LogError("Trying to access out of bounds info");
                return "";
        }
    }
}
