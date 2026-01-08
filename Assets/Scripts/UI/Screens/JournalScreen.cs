using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class JournalScreen : IScreen
{
    private VisualTreeAsset tree;
    private VisualElement root;

    private VisualElement screenElements;
    private bool firstFrame;

    private List<CreatureProfile> profiles;
    public void Initialize(VisualTreeAsset tree, VisualElement root)
    {
        this.tree = tree;

        screenElements = tree.Instantiate();
        root.Add(screenElements);

        this.root = root;

        FillCreatureProfiles();

        SubscribeButtons();

        firstFrame = true;
    }

    public void Update()
    {
        if( (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape)) && !firstFrame)
        {
            GameManager.Instance.ToggleTimeStopped(false);
            UIManager.Instance.ChangeScreen(UIManager.Instance.lastScreen);
        }

        firstFrame = false;
    }


    private void FillCreatureProfiles()
    { 
        profiles = GameManager.Instance.GetSaveFile().journal.GetCreatureProfiles();

        for (int i = 0; i < profiles.Count; i++)
        {
            VisualElement creatureProfile = UIManager.Instance.creatureProfileTemplate.CloneTree();

            VisualElement parent;

            switch (profiles[i].GetZone())
            {
                case ZoneID.Sunlight:
                    parent = root.Q<VisualElement>("SunlightCreatureContainer");
                    break;
                case ZoneID.Twilight:
                    parent = root.Q<VisualElement>("TwilightCreatureContainer");
                    break;
                case ZoneID.Midnight:
                    parent = root.Q<VisualElement>("MidnightCreatureContainer");
                    break;
                default:
                    parent = root.Q<VisualElement>("SunlightCreatureContainer");
                    break;
            }

            creatureProfile.userData = profiles[i];

            creatureProfile.Q<Label>("CreatureName").text = profiles[i].name;
            creatureProfile.Q<VisualElement>("CreatureImage").style.backgroundImage = new StyleBackground(profiles[i].GetImage());


            // Max Level Info
            if (profiles[i].GetLevel() == 3)
            {
                // Should make it green, but no work
                creatureProfile.style.unityBackgroundImageTintColor = new Color(29, 255, 114);
                creatureProfile.Q<VisualElement>("CreatureImage").style.unityBackgroundImageTintColor = Color.white;
                creatureProfile.Q<VisualElement>("ResearchProgress").style.display = DisplayStyle.None;
                creatureProfile.Q<Label>("MaxLevelLabel").style.display = DisplayStyle.Flex;
            }
            else
            {
                // Anything between 0 and max level info

                ProgressBar creatureProgress = creatureProfile.Q<ProgressBar>("ProgressBar");

                creatureProgress.lowValue = profiles[i].GetMinValue();
                creatureProgress.highValue = profiles[i].GetMaxValue();

                creatureProgress.value = profiles[i].GetProgress();

                if (profiles[i].GetLevel() > 0)
                {
                    creatureProfile.Q<VisualElement>("CreatureImage").style.unityBackgroundImageTintColor = Color.white;
                    creatureProfile.Q<VisualElement>("ResearchProgress").style.display = DisplayStyle.Flex;
                    creatureProfile.Q<Label>("MaxLevelLabel").style.display = DisplayStyle.None;

                    creatureProfile.Q<Label>("CurLevel").text = "" + profiles[i].GetLevel();
                    creatureProfile.Q<Label>("NextLevel").text = "" + (profiles[i].GetLevel() + 1);
                }
                // Level 0 info
                else
                {
                    creatureProfile.Q<Label>("CreatureName").text = "???";

                    creatureProfile.Q<VisualElement>("CreatureImage").style.unityBackgroundImageTintColor = Color.black;
                    creatureProfile.Q<VisualElement>("ResearchProgress").style.display = DisplayStyle.Flex;
                    creatureProfile.Q<Label>("MaxLevelLabel").style.display = DisplayStyle.None;

                    creatureProfile.Q<Label>("CurLevel").text = "" + 0;
                    creatureProfile.Q<Label>("NextLevel").text = 1.ToString();
                }
            }

            Button button = creatureProfile.Q<Button>("CreatureProfileContainer");

            CreatureProfile currentProfile = profiles[i];
            button.clicked += () => OnCreatureProfileClicked(currentProfile);
            button.clicked += () => SoundManager.Instance.PlaySound(SoundManager.SoundID.Click);

            button.RegisterCallback<PointerEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));

            parent.Add(creatureProfile);
        }
    }

    private void SubscribeButtons()
    {
        root.Q<Button>("BackButton").clicked += OnBackButtonClicked;
    }

    private void OnCreatureProfileClicked(CreatureProfile creatureProfile)
    {
        VisualElement popup = root.Q<VisualElement>("CreatureInfoPopUp");

        popup.style.display = DisplayStyle.Flex;

        root.Q<Label>("InfoBoxCreatureName").text = creatureProfile.name;
        root.Q<VisualElement>("InfoBoxCreatureImage").style.backgroundImage = new StyleBackground(creatureProfile.GetImage());

        // Max Level Info
        if (creatureProfile.GetLevel() == 3)
        {
            popup.Q<VisualElement>("InfoBoxCreatureImage").style.unityBackgroundImageTintColor = Color.white;
            popup.Q<VisualElement>("ResearchProgressBarContainer").style.display = DisplayStyle.None;
            popup.Q<Label>("MaxResearchLevelLabel").style.display = DisplayStyle.Flex;
        }
        else
        {
            // Anything between 0 and max level info

            ProgressBar creatureProgress = popup.Q<ProgressBar>("ResearchProgressBar");

            creatureProgress.lowValue = creatureProfile.GetMinValue();
            creatureProgress.highValue = creatureProfile.GetMaxValue();

            creatureProgress.value = creatureProfile.GetProgress();

            if (creatureProfile.GetLevel() > 0)
            {
                popup.Q<VisualElement>("InfoBoxCreatureImage").style.unityBackgroundImageTintColor = Color.white;
                popup.Q<VisualElement>("ResearchProgressBarContainer").style.display = DisplayStyle.Flex;
                popup.Q<Label>("MaxResearchLevelLabel").style.display = DisplayStyle.None;

                popup.Q<Label>("CurResearchLevelLabel").text = "" + creatureProfile.GetLevel();
                popup.Q<Label>("NextResearchLevelLabel").text = "" + (creatureProfile.GetLevel() + 1);
            }
            // Level 0 info
            else
            {
                popup.Q<Label>("InfoBoxCreatureName").text = "???";

                popup.Q<VisualElement>("InfoBoxCreatureImage").style.unityBackgroundImageTintColor = Color.black;
                popup.Q<VisualElement>("ResearchProgressBarContainer").style.display = DisplayStyle.Flex;
                popup.Q<Label>("MaxResearchLevelLabel").style.display = DisplayStyle.None;

                popup.Q<Label>("CurResearchLevelLabel").text = "" + 0;
                popup.Q<Label>("NextResearchLevelLabel").text = "" + 1;
            }
        }

        // NOT DONE OMG THIS ISNT DONE OMG
        popup.Q<Label>("CategoryInfoLabel").text = "";

        // ENDANGERMENT BAR PLEASE PLEASE PLASE ITS CALLED "EndangermentBar"

        int curLevel = creatureProfile.GetLevel();

        for (int i = 1; i < 3; i++)
        {
            VisualElement curInfoBox = root.Q<VisualElement>("FishInfo" + i);

            if(i <= curLevel)
            {
                curInfoBox.Q<VisualElement>("InfoBackground").style.display = DisplayStyle.Flex;
                curInfoBox.Q<VisualElement>("InfoLockedBackground").style.display = DisplayStyle.None;

                curInfoBox.Q<Label>("InfoLabel").text = creatureProfile.GetInfo(i);
            }
            else
            {
                curInfoBox.Q<VisualElement>("InfoBackground").style.display = DisplayStyle.None;
                curInfoBox.Q<VisualElement>("InfoLockedBackground").style.display = DisplayStyle.Flex;
            }
        }
    }

    private void OnBackButtonClicked()
    {
        root.Q<VisualElement>("CreatureInfoPopUp").style.display = DisplayStyle.None;
    }
}
