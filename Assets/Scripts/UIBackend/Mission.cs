using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Mission", menuName = "Mission")]
public class Mission : ScriptableObject
{
    [Header("")]
    public List<CreatureID> creatureNeeded;

    [Header("Meta Information")]
    public MissionID id;
    public bool oneTime;

    public string title;
    [TextArea(5, 20)]
    public string description;
    
    [UnityEngine.Range(0,2)]
    public int difficulty;
    public Sprite missionImage;
    public ResearcherID researcher;

    [Header("Reward")]
    [TextArea(5, 20)]
    public string rewardText;
    [TextArea(5, 20)]
    public string failText;
    public float missionRewardFlat;

    [Header("Unlock Conditions")]
    [SerializeField] private List<UnlockCondition> unlockConditions;

    [HideInInspector]
    public int completedAmount;

    public bool GetUnlocked()
    {
        if (completedAmount >= 1 && oneTime) return false;

        if (unlockConditions.Count == 0) return true;

        for (int i = 0; i < unlockConditions.Count; i++)
        {
            if (unlockConditions[i].GetUnlocked() == false) return false;
        }
        return true;
    }

    public void ResetMission()
    {
        completedAmount = 0;
    }

    public Mission GenerateGenericMission()
    {
        Mission newMission = Instantiate(this);

        CreatureID creature = (CreatureID)Random.Range(1, System.Enum.GetValues(typeof(CreatureID)).Length);

        newMission.creatureNeeded.Add(creature);

        CreatureProfile creatureProfile = GameManager.Instance.GetSaveFile().journal.GetCreatureProfile(creature);

        if(creatureProfile == null) Debug.Log("Chosen Creature: " + creature);

        newMission.title = newMission.title.Replace("[species]", creatureProfile.GetName());
        newMission.title = newMission.title.Replace("[latin name]", creatureProfile.GetLatinName());

        newMission.description = newMission.description.Replace("[species]", creatureProfile.GetName());
        newMission.description = newMission.description.Replace("[latin name]", creatureProfile.GetLatinName());

        return newMission;
    }
}

