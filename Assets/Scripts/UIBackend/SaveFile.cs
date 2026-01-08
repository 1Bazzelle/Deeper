using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New SaveFile", menuName = "SaveFile")]
public class SaveFile : ScriptableObject
{
    public UpgradeData upgradeData;
    public MissionPool missionPool;
    public ResearcherContainer researcherContainer;
    public Journal journal;

    [HideInInspector]
    public List<Mission> currentMissionSelection = new();

    public void ResetData()
    {
        upgradeData.Initialize();

        missionPool.ResetMissions();

        researcherContainer.ResetProgress();

        journal.ResetProgress();
    }
}
