using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchBaseScreen : IScreen
{
    private VisualTreeAsset tree;
    private VisualElement root;

    private VisualElement screenElements;

    private List<VisualElement> missionSlots;
    public void Initialize(VisualTreeAsset tree, VisualElement root)
    {
        this.tree = tree;

        screenElements = tree.Instantiate();
        root.Add(screenElements);

        this.root = root;

        SubscribeButtons();

        #region Mission Initialization
        for (int i = 0; i < GameManager.Instance.GetSaveFile().currentMissionSelection.Count; i++)
        {
            VisualElement mission = UIManager.Instance.missionTemplate.CloneTree();

            mission.Q<Label>("MissionTitle").text = GameManager.Instance.GetSaveFile().currentMissionSelection[i].title;

            mission.userData = GameManager.Instance.GetSaveFile().currentMissionSelection[i];

            Button missionButton = mission.Q<Button>("MissionContainer");

            int currentIndex = i;

            missionButton.clicked += () => OnMissionButtonClicked(GameManager.Instance.GetSaveFile().currentMissionSelection[currentIndex]);
            missionButton.clicked += () => SoundManager.Instance.PlaySound(SoundManager.SoundID.Click);

            missionButton.RegisterCallback<PointerEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));

            root.Q<ScrollView>("MissionsScrollView").Add(mission);
        }
        #endregion  

        #region Mission Slots
        missionSlots = new()
        {
            root.Q<VisualElement>("MissionSlot1"),
            root.Q<VisualElement>("MissionSlot2"),
            root.Q<VisualElement>("MissionSlot3")
        };

        missionSlots[0].Q<Button>("MissionSlotCloseButton").clicked += CloseMissionSlot1;
        missionSlots[1].Q<Button>("MissionSlotCloseButton").clicked += CloseMissionSlot2;
        missionSlots[2].Q<Button>("MissionSlotCloseButton").clicked += CloseMissionSlot3;

        UpdateMissionSlots();
        #endregion
    }
    public void Update() { }
    private void SubscribeButtons()
    {
        RegisterButton("MissionsTabButton", OnMissionsTabClicked);
        RegisterButton("UpgradesTabButton", OnUpgradesTabClicked);
        RegisterButton("EmbarkButton", OnEmbarkButtonClicked);
    }

    private void RegisterButton(string buttonName, Action callback)
    {
        Button button = root.Q<Button>(buttonName);

        if (button != null)
        {
            button.clicked += () =>
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundID.Click);
                callback();
            };
            button.RegisterCallback<PointerEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));
        }
        else
        {
            Debug.LogWarning("Button not found");
        }
    }


    #region Clicked Events
    private void OnMissionsTabClicked()
    {
        root.Q<VisualElement>("UpgradesTab").style.display = DisplayStyle.None;
        root.Q<VisualElement>("MissionsTab").style.display = DisplayStyle.Flex;
    }
    private void OnUpgradesTabClicked()
    {
        root.Q<VisualElement>("MissionsTab").style.display = DisplayStyle.None;
        root.Q<VisualElement>("UpgradesTab").style.display = DisplayStyle.Flex;
    }

    private void OnMissionButtonClicked(Mission mission)
    {
        if (missionSlots[0].userData == null)
        {
            AssignDataToSlot(missionSlots[0], mission);
        }
        else if (missionSlots[1].userData == null && GameManager.Instance.GetSaveFile().upgradeData.GetUnlockStatus(UpgradeID.AddMissionSlot1))
        {
            AssignDataToSlot(missionSlots[1], mission);
        }
        else if (missionSlots[2].userData == null && GameManager.Instance.GetSaveFile().upgradeData.GetUnlockStatus(UpgradeID.AddMissionSlot2))
        {
            AssignDataToSlot(missionSlots[2], mission);
        }
    }

    private void OnEmbarkButtonClicked()
    {
        GameManager.Instance.ChangeGameState(new StateSubmerged());
    }
    #endregion

    private void AssignDataToSlot(VisualElement missionSlot, Mission data)
    {
        if (GameManager.Instance.descent.selectedMissions.Contains(data)) return;

        missionSlot.userData = data;

        missionSlot.Q<VisualElement>("MissionInfoContainer").style.display = DisplayStyle.Flex;
        missionSlot.Q<VisualElement>("NoMissionInfoContainer").style.display = DisplayStyle.None;
        missionSlot.Q<VisualElement>("MissionSlotLockedContainer").style.display = DisplayStyle.None;

        missionSlot.Q<VisualElement>("MissionImage").style.backgroundImage = new StyleBackground(data.missionImage);

        #region Difficulty
        if (data.difficulty == 0)
        {
            missionSlot.Q<VisualElement>("Difficulty1").Q<VisualElement>("Active").style.display = DisplayStyle.Flex;
            missionSlot.Q<VisualElement>("Difficulty1").Q<VisualElement>("Inactive").style.display = DisplayStyle.None;
        }
        else
        {
            missionSlot.Q<VisualElement>("Difficulty1").Q<VisualElement>("Active").style.display = DisplayStyle.None;
            missionSlot.Q<VisualElement>("Difficulty1").Q<VisualElement>("Inactive").style.display = DisplayStyle.Flex;
        }
        if (data.difficulty >= 1)
        {
            missionSlot.Q<VisualElement>("Difficulty2").Q<VisualElement>("Active").style.display = DisplayStyle.Flex;
            missionSlot.Q<VisualElement>("Difficulty2").Q<VisualElement>("Inactive").style.display = DisplayStyle.None;
        }
        else
        {
            missionSlot.Q<VisualElement>("Difficulty2").Q<VisualElement>("Active").style.display = DisplayStyle.None;
            missionSlot.Q<VisualElement>("Difficulty2").Q<VisualElement>("Inactive").style.display = DisplayStyle.Flex;
        }
        if (data.difficulty >= 2)
        {
            missionSlot.Q<VisualElement>("Difficulty3").Q<VisualElement>("Active").style.display = DisplayStyle.Flex;
            missionSlot.Q<VisualElement>("Difficulty3").Q<VisualElement>("Inactive").style.display = DisplayStyle.None;
        }
        else
        {
            missionSlot.Q<VisualElement>("Difficulty3").Q<VisualElement>("Active").style.display = DisplayStyle.None;
            missionSlot.Q<VisualElement>("Difficulty3").Q<VisualElement>("Inactive").style.display = DisplayStyle.Flex;
        }
        #endregion

        missionSlot.Q<Label>("MissionTitle").text = data.title;
        missionSlot.Q<Label>("MissionDescription").text = data.description;

        Researcher curResearcher = GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(data.researcher);

        missionSlot.Q<VisualElement>("ResearcherImage").style.backgroundImage = new StyleBackground(curResearcher.image);

        GameManager.Instance.descent.selectedMissions.Add(data);
    }


    private void UpdateMissionSlots()
    {
        for (int i = 0; i < missionSlots.Count; i++)
        {
            var missionSlot = missionSlots[i];

            // Check if the mission slot is unlocked
            bool isUnlocked = i == 0 || GameManager.Instance.GetSaveFile().upgradeData.GetUnlockStatus((UpgradeID)(UpgradeID.AddMissionSlot1 + i - 1));

            if (!isUnlocked)
            {
                // If not unlocked, show the locked container
                ShowContainer(missionSlot, "Locked");
            }
            else if (missionSlot.userData == null)
            {
                // If unlocked but no mission is assigned, show the "no mission" container
                ShowContainer(missionSlot, "Empty");
            }
            else
            {
                // If unlocked and mission is assigned, show the mission info container
                ShowContainer(missionSlot, "MissionInfo");
            }
        }
    }

    private void ShowContainer(VisualElement missionSlot, string containerName)
    {
        missionSlot.Q<VisualElement>("MissionInfoContainer").style.display = containerName == "MissionInfo" ? DisplayStyle.Flex : DisplayStyle.None;
        missionSlot.Q<VisualElement>("NoMissionInfoContainer").style.display = containerName == "Empty" ? DisplayStyle.Flex : DisplayStyle.None;
        missionSlot.Q<VisualElement>("MissionSlotLockedContainer").style.display = containerName == "Locked" ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void CloseMissionSlot1() => CloseMissionSlot(0);
    private void CloseMissionSlot2() => CloseMissionSlot(1);
    private void CloseMissionSlot3() => CloseMissionSlot(2);
    private void CloseMissionSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= missionSlots.Count) return;

        Mission mission = (Mission)missionSlots[slotIndex].userData;
        GameManager.Instance.descent.selectedMissions.Remove(mission);

        missionSlots[slotIndex].userData = null;

        UpdateMissionSlots();
    }
}
