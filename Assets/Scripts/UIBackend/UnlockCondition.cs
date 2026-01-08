using UnityEngine;

public enum ConditionType
{
    None,
    Upgrade,
    Researcher,
    CreatureProgress,
    SpecificMission
}

[System.Serializable]
public struct UnlockCondition
{
    [SerializeField] private ConditionType type;

    [SerializeField] private UpgradeID upgradeNecessary;

    [SerializeField] private ResearcherID researcher;
    [SerializeField] private int researcherProgressNecessary;

    [SerializeField] private CreatureID creature;
    [SerializeField] private int creatureProgressNecessary;

    [SerializeField] private MissionID otherMission;
    public bool GetUnlocked()
    {
        if (type == ConditionType.None) return true;
        switch (type)
        {
            case ConditionType.Upgrade:

                return GameManager.Instance.GetSaveFile().upgradeData.GetUnlockStatus(upgradeNecessary);

            case ConditionType.Researcher:

                Researcher res = GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(researcher);
                if (res == null) return false;

                Researcher newRes = (Researcher)res;

                return researcherProgressNecessary < newRes.progress;

            case ConditionType.CreatureProgress:

                CreatureProfile profile = GameManager.Instance.GetSaveFile().journal.GetCreatureProfile(creature);
                if (profile == null) return false;

                return creatureProgressNecessary < profile.GetProgress();

            case ConditionType.SpecificMission:

                return GameManager.Instance.GetSaveFile().missionPool.GetMission(otherMission).completedAmount > 0;
            default:
                return false;
        }
    }
}