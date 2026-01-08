using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public struct UpgradeData
{
    [System.Serializable]
    private struct Upgrade
    {
        public UpgradeID id;
        public bool status;

        public Upgrade(UpgradeID id, bool status)
        {
            this.id = id;
            this.status = status;
        }
    }

    [SerializeField] private List<Upgrade> upgrades;

    public void Initialize()
    {
        upgrades = new List<Upgrade>();

        foreach (UpgradeID upgrade in Enum.GetValues(typeof(UpgradeID)))
        {
            upgrades.Add(new(upgrade, false));
        }

        SetUnlockStatus(UpgradeID.AddMissionSlot1, true);
    }
    public void ResetProgress()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade temp;

            temp.id = upgrades[i].id;
            temp.status = false;

            upgrades[i] = temp;
        }
    }
    public bool GetUnlockStatus(UpgradeID upgradeID)
    {

        for (int i = 0; i < upgrades.Count; i++)
        {
            if(upgradeID == upgrades[i].id)
            {
                return upgrades[i].status;
            }
        }

        return false;
    }

    public void SetUnlockStatus(UpgradeID id, bool newStatus)
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (id == upgrades[i].id)
            {
                Upgrade upgrade = upgrades[i];
                upgrade.status = newStatus;
                upgrades[i] = upgrade;

            }
        }
    }
}
