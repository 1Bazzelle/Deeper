using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MissionPool
{
    private List<Mission> availableMissions = new();

    [SerializeField] private List<Mission> allMissions;

    public void UpdateAvailableMissions()
    {
        availableMissions.Clear();
        for (int i = 0; i < allMissions.Count; i++)
        {
            // Debug.Log(allMissions[i]);
            if (allMissions[i].GetUnlocked() == true) availableMissions.Add(allMissions[i]);
        }
    }

    public List<Mission> GetAvailableMissions()
    {
        return availableMissions;
    }

    public void ResetMissions()
    {
        for (int i = 0; i < allMissions.Count; i++)
        {
            allMissions[i].ResetMission();
        }
    }

    public Mission GetMission(MissionID id)
    {
        for (int i = 0; i < allMissions.Count; i++)
        {
            if (allMissions[i].id == id) return allMissions[i];
        }
        return null;
    }
}
