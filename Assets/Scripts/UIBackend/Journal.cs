using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[CreateAssetMenu(fileName = "New Journal", menuName = "Journal")]
public class Journal : ScriptableObject
{
     [SerializeField] private List<CreatureProfile> profiles;

    public CreatureProfile GetCreatureProfile(CreatureID id)
    {
        bool found = false;
        int index = 0;

        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].id == id)
            {
                found = true;
                index = i;
                break;
            }
        }
        if(found)
        return profiles[index];
        return null;
    }
    public List<CreatureProfile> GetCreatureProfiles()
    {
        return profiles;
    }

    public void ResetProgress()
    {
        for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].progress = 0;
        }
    }
}