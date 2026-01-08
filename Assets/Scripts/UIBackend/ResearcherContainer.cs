using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New ResearcherContainer", menuName = "ResearcherContainer")]
public class ResearcherContainer : ScriptableObject
{
    [SerializeField] private List<Researcher> researchers;

    public Researcher GetResearcher(ResearcherID researcherID)
    {
        for (int i = 0; i < researchers.Count; i++)
        {
            if (researchers[i].id == researcherID) return researchers[i];
        }
        Debug.LogWarning("RESEARCHER WAS NOT FOUND, I REPEAT, NOT FOUND");
        return null;
    }

    public void ResetProgress()
    {
        for (int i = 0; i < researchers.Count; i++)
        {
            Researcher res = researchers[i];

            res.id = researchers[i].id;
            res.name = researchers[i].name;
            res.image = researchers[i].image;

            res.progress = 0;

            researchers[i] = res;
        }
    }
}

[System.Serializable]
public class Researcher
{
    public enum RewardType
    {
        None,
        Funds,
        ResearchPoints
    }
    public ResearcherID id;
    public string name;
    public Sprite image;
    public RewardType rewardType;
    public float rewardBonus;

    [HideInInspector] public int progress;
}