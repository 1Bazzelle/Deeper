using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceLines", menuName = "Systems/VoiceLines")]
public class VoiceLines : ScriptableObject
{
    [SerializeField] private List<FirstPictureVoiceLine> firstPictureVoiceLines;
    [SerializeField] private List<VoiceLine> fillerVoiceLines;

    public VoiceLine GetVoiceLine(CreatureID creature)
    {
        for (int i = 0; i < firstPictureVoiceLines.Count; i++)
        {
            if (firstPictureVoiceLines[i].creature == creature)
            {
                if (!firstPictureVoiceLines[i].triggered)
                {
                    FirstPictureVoiceLine temp = firstPictureVoiceLines[i];
                    temp.triggered = true;
                    firstPictureVoiceLines[i] = temp;

                    return firstPictureVoiceLines[i].voiceLine;
                }
                break;
            }
        }

        return GetFillerVoiceLine();
    }

    public VoiceLine GetFillerVoiceLine()
    {
        return fillerVoiceLines[Random.Range(0, fillerVoiceLines.Count)];
    }

    public void ResetVoiceLines()
    {
        for (int i = 0; i < firstPictureVoiceLines.Count; i++)
        {
            FirstPictureVoiceLine temp = firstPictureVoiceLines[i];
            temp.triggered = false;
            firstPictureVoiceLines[i] = temp;
        }
    }
}
