using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public struct VoiceLine
{
    public AudioClip clip;
    public string subtitles;
}
[System.Serializable]
public struct FirstPictureVoiceLine
{
    public CreatureID creature;
    public VoiceLine voiceLine;
    [HideInInspector]
    public bool triggered;
}
[System.Serializable]
public struct Sound
{
    public SoundManager.SoundID id;
    public AudioClip clip;
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        #endregion
    }

    public enum SoundID
    {
        None,
        Click,
        Hover,
        Discard,
        ChooseMission,
        Embark,
        ReturnToBase
    }
    [SerializeField] private AudioSource dynamicSource;
    [SerializeField] private List<Sound> dynamicSounds;

    [Header("")]
    [SerializeField] private AudioSource voiceLineSource;
    [SerializeField] private VoiceLines voiceLines;
    [Tooltip("In Seconds")]
    [SerializeField] private float voiceLineCooldown;
    private float voiceLineCooldownTimer;

    [Header("Ambience")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip underwaterAmbience;
    [Header("")]
    [SerializeField] private AudioSource whaleSource;
    [SerializeField] private List<AudioClip> whaleSounds;
    [Tooltip("In Seconds")]
    [SerializeField] private float whaleSoundFrequency;
    [UnityEngine.Range(0, 1)]
    [SerializeField] private float whaleSoundChance;
    private float whaleSoundTimer;
    [Header("")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioClip> music;
    [Tooltip("In Seconds")]
    [SerializeField] private float musicFrequency;
    [UnityEngine.Range(0, 1)]
    [SerializeField] private float musicChance;
    private float musicTimer;

    private bool submerged;
    void OnEnable()
    {
        ambienceSource.clip = underwaterAmbience;
    }
    void Update()
    {
        if(submerged)
        {
            dynamicSource.transform.position = PlayerManager.Instance.GetPlayerTransform().position;

            if (!ambienceSource.isPlaying) ambienceSource.Play();

            #region Music
            if (musicTimer <= 0 && !musicSource.isPlaying)
            {
                if(Random.Range(0, 1) < musicChance) musicSource.PlayOneShot(music[Random.Range(0, music.Count)]);
                musicTimer = musicFrequency;
            }
            musicTimer -= Time.deltaTime;
            #endregion

            #region WhaleSounds
            if (whaleSoundTimer <= 0)
            {
                if (Random.Range(0, 1) < whaleSoundChance) whaleSource.PlayOneShot(whaleSounds[Random.Range(0, whaleSounds.Count)]);
            }
            whaleSoundTimer -= Time.deltaTime;

            #endregion
        }
        else
        {
            dynamicSource.transform.position = PlayerManager.Instance.GetResearchBaseTransform().position;

            ambienceSource.Stop();
            whaleSource.Stop();
        }

        voiceLineCooldownTimer -= Time.deltaTime;
    }

    public void TogglePlayerSubmerged(bool state)
    {
        submerged = state;
        if(state)
        {
            ambienceSource.clip = underwaterAmbience;
            ambienceSource.Play();

            musicTimer = musicFrequency;
            whaleSoundTimer = whaleSoundFrequency;
        }
    }
    public void PlayVoiceLine(CreatureID creature)
    {
        if (!voiceLineSource.isPlaying && voiceLineCooldownTimer <= 0)
        { 
            VoiceLine voiceLine = voiceLines.GetVoiceLine(creature);
            voiceLineSource.PlayOneShot(voiceLine.clip);
            voiceLineCooldownTimer = voiceLineCooldown;
        }

    }

    public void PlaySound(SoundID id)
    {
        for (int i = 0; i < dynamicSounds.Count; i++)
        { 
            if (dynamicSounds[i].id == id)
            {
                dynamicSource.PlayOneShot(dynamicSounds[i].clip);
                return;
            }
        }
    }

    public void ResetVoiceLines()
    {
        voiceLines.ResetVoiceLines();
    }
}