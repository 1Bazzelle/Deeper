using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public struct Picture
{
    public Sprite sprite;
    public List<(CreatureID creature, int quality)> content;
    public ZoneID zone;

    public Picture(Sprite sprite, List<(CreatureID creature, int quality)> content, ZoneID zone)
    {
        this.sprite = sprite;
        this.content = content;
        this.zone = zone;
    }

    public bool CreatureOnPicture(CreatureID creatureID)
    {
        for (int i = 0; i < content.Count; i++)
        {
            if (content[i].creature == creatureID) return true;
        }
        return false;
    }

    public CreatureID GetTopFish()
    {
        CreatureID topFish = new();
        float curBestScore = 0;

        for (int i = 0; i < content.Count; i++)
        {
            if (content[i].quality > curBestScore)
            {
                topFish = content[i].creature;
                curBestScore = content[i].quality;
            }
        }

        return topFish;
    }
}
