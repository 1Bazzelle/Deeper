using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MissionCompletionScreen : IScreen
{
    private struct MissionData
    {
        public Mission mission;
        public Picture? savedPicture;

        private bool succeeded;

        public bool CalculateSuccessStatus()
        {
            succeeded = false;
            for (int i = 0; i < mission.creatureNeeded.Count; i++)
            {
                if (savedPicture.Value.CreatureOnPicture(mission.creatureNeeded[i]))
                {
                    succeeded = true;
                }
            }

            return succeeded;
        }
    }

    private List<MissionData> missionData;

    private VisualTreeAsset tree;
    private VisualElement root;

    private VisualElement screenElements;

    private Descent curDescent;
    private int curMissionIndex = 0;

    private int scoreMultiplier = 100;

    private struct CreatureResult
    {
        public CreatureID creature;
        public int score;
        public int bonusScore;
    }

    List<CreatureResult> researchResults;
    private void AddScore(CreatureID id, int score)
    {
        for (int i = 0; i < researchResults.Count; i++)
        {
            if (researchResults[i].creature == id)
            {
                CreatureResult result = researchResults[i];

                // This decides how much a star counts into creature research
                result.score += score * scoreMultiplier;

                researchResults[i] = result;
                return;
            }
        }
        CreatureResult newCreature = new CreatureResult();
        newCreature.creature = id;
        newCreature.score = score;
        researchResults.Add(newCreature);
    }
    private void AddBonuses()
    {
        // For each fish in the Research Results
        for (int i = 0; i < researchResults.Count; i++)
        {
            // For each mission
            for (int p = 0; p < missionData.Count; p++)
            {
                // that was successful
                if (missionData[p].CalculateSuccessStatus())
                {
                    Researcher curResearcher = GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(missionData[p].mission.researcher);

                    // If the current Researcher gives extra research points on fish
                    if (curResearcher.rewardType == Researcher.RewardType.ResearchPoints)
                    {
                        // Search through the needed Creatures of that mission
                        for (int m = 0; m < missionData[p].mission.creatureNeeded.Count; m++)
                        {
                            // And check if it contains our current fish from the Research Results
                            if (missionData[p].mission.creatureNeeded[m] == researchResults[i].creature)
                            {
                                CreatureResult curResult = researchResults[i];
                                curResult.bonusScore = (int) ((researchResults[i].score * curResearcher.rewardBonus) - researchResults[i].score);
                                researchResults[i] = curResult;
                            }
                        }
                    }
                }
            }
        }
    }

    private int curFishIndex = 0;
    private int curPopUpMissionIndex = 0;

    public void Initialize(VisualTreeAsset tree, VisualElement root)
    {
        this.tree = tree;

        screenElements = tree.Instantiate();
        root.Add(screenElements);

        this.root = root;

        missionData = new();

        curDescent = GameManager.Instance.descent;

        SubscribeButtons();

        FillPictures();

        #region Create MissionData

        for (int i = 0; i < curDescent.selectedMissions.Count; i++)
        {
            MissionData newMission = new MissionData();

            newMission.mission = curDescent.selectedMissions[i];

            missionData.Add(newMission);
        }

        #endregion

        UpdateMissions();

        UpdateSubmitButton();

        root.Q<VisualElement>("PopUpContainer").style.display = DisplayStyle.None;

        if (missionData.Count == 0) root.Q<Button>("SubmitButton").pickingMode = PickingMode.Position;
        else root.Q<Button>("SubmitButton").pickingMode = PickingMode.Ignore;
    }
    public void Update() { }
    private void SubscribeButtons()
    {
        RegisterButton("SubmitButton", OnSubmitButtonClicked);
        RegisterButton("PrevMissionButton", OnPrevMissionButtonClicked);
        RegisterButton("NextMissionButton", OnNextMissionButtonClicked);
        RegisterButton("PrevFishButton", OnPrevFishButtonClicked);
        RegisterButton("NextFishButton", OnNextFishButtonClicked);

        RegisterButton("PopUpContainer", "PrevMissionButton", OnPrevMissionClickedPopUp);
        RegisterButton("PopUpContainer", "NextMissionButton", OnNextMissionClickedPopUp);
        RegisterButton("PopUpContainer", "ContinueButton", OnContinueClicked);
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
    }

    private void RegisterButton(string containerName, string buttonName, Action callback)
    {
        VisualElement container = root.Q<VisualElement>(containerName);
        if (container != null)
        {
            Button button = container.Q<Button>(buttonName);
            if (button != null)
            {
                button.clicked += () =>
                {
                    SoundManager.Instance.PlaySound(SoundManager.SoundID.Click);
                    callback();
                };

                button.RegisterCallback<PointerEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));
            }
        }
    }

    #region Button Click Responses

    #region Picture Submission
    private void OnPrevMissionButtonClicked()
    {
        if (curDescent.selectedMissions.Count != 0) curMissionIndex = (curMissionIndex - 1 + curDescent.selectedMissions.Count) % curDescent.selectedMissions.Count;
        UpdateMissions();
    }
    private void OnNextMissionButtonClicked()
    {
        if (curDescent.selectedMissions.Count != 0) curMissionIndex = (curMissionIndex + 1) % curDescent.selectedMissions.Count;
        UpdateMissions();
    }
    private void OnPictureSlotClicked(Picture picture)
    {
        root.Q<VisualElement>("SelectedPicture").style.backgroundImage = new StyleBackground(picture.sprite);

        if (curDescent.selectedMissions.Count != 0)
        {
            MissionData mission = missionData[curMissionIndex];
            mission.savedPicture = picture;
            missionData[curMissionIndex] = mission;
        }

        UpdateSubmitButton();
    }
    private void OnSubmitButtonClicked()
    {
        CalculateResults();
        root.Q<VisualElement>("PopUpContainer").style.display = DisplayStyle.Flex;
    }
    #endregion

    #region Results Pop Up
    private void OnPrevFishButtonClicked()
    {
        if(researchResults.Count != 0) curFishIndex = (curFishIndex - 1 + researchResults.Count) % researchResults.Count;
        UpdateFishResearch();
    }
    private void OnNextFishButtonClicked()
    {
        if (researchResults.Count != 0) curFishIndex = (curFishIndex + 1) % researchResults.Count;
        UpdateFishResearch();
    }
    private void OnPrevMissionClickedPopUp()
    {
        if (curDescent.selectedMissions.Count != 0) curPopUpMissionIndex = (curPopUpMissionIndex - 1 + curDescent.selectedMissions.Count) % curDescent.selectedMissions.Count;
        UpdateMissionResults();
    }
    private void OnNextMissionClickedPopUp()
    {
        if (curDescent.selectedMissions.Count != 0) curPopUpMissionIndex = (curPopUpMissionIndex + 1) % curDescent.selectedMissions.Count;
        UpdateMissionResults();
    }
    private void OnContinueClicked()
    {
        ApplyResults();
        GameManager.Instance.ChangeGameState(new StateResearchBase());
    }
    #endregion

    #endregion

    #region Picture Submission
    private void FillPictures()
    {
        for (int i = 0; i < curDescent.takenPictures.Count; i++)
        {
            VisualElement pictureSlot = UIManager.Instance.pictureSlotTemplate.CloneTree();

            Button button = pictureSlot.Q<Button>("PictureSlotButton");

            button.style.backgroundImage = new StyleBackground(curDescent.takenPictures[i].sprite.texture);
            pictureSlot.userData = curDescent.takenPictures[i];

            int curIndex = i;

            button.clicked += () => OnPictureSlotClicked(curDescent.takenPictures[curIndex]);

            button.clicked += () => SoundManager.Instance.PlaySound(SoundManager.SoundID.Click);
            button.RegisterCallback<PointerEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));

            root.Q<VisualElement>("PictureContainer").Add(pictureSlot);
        }
    }
    private void UpdateMissions()
    {
        if (missionData.Count != 0)
        {
            root.Q<Label>("MissionIndex").text = (curMissionIndex + 1).ToString() + "/ " + curDescent.selectedMissions.Count.ToString();

            root.Q<Label>("MissionTitle").text = missionData[curMissionIndex].mission.title;

            root.Q<Label>("MissionDescription").text = missionData[curMissionIndex].mission.description;

            #region Images

            VisualElement researcherImage = root.Q<VisualElement>("ResearcherImage");

            Sprite researcherSprite = GameManager.Instance.GetSaveFile().researcherContainer
            .GetResearcher(missionData[curMissionIndex].mission.researcher).image;

            researcherImage.style.backgroundImage = new StyleBackground(researcherSprite.texture);


            if (missionData[curMissionIndex].mission.missionImage != null)
            {
                VisualElement missionImage = root.Q<VisualElement>("MissionImage");

                Sprite missionSprite = missionData[curMissionIndex].mission.missionImage;

                missionImage.style.backgroundImage = new StyleBackground(missionSprite.texture);
            }
            else
            {
                VisualElement missionImage = root.Q<VisualElement>("MissionImage");

                Sprite o = null;
                missionImage.style.backgroundImage = new StyleBackground(o);

            }

            if (missionData[curMissionIndex].savedPicture != null)
            {
                root.Q<VisualElement>("SelectedPicture").style.backgroundImage = new StyleBackground(missionData[curMissionIndex].savedPicture.Value.sprite);
            }
            else
            {
                root.Q<VisualElement>("SelectedPicture").style.backgroundImage = new StyleBackground(UIManager.Instance.emptyPictureSlot);
            }
            #endregion
        }
        else
        {
            root.Q<Label>("MissionIndex").text = " ";

            root.Q<Label>("MissionTitle").text = "Oh there you are.";

            root.Q<Label>("MissionDescription").text = "What were you doing down there? You've got a job to do, are you aware of that?";

            #region Images

            VisualElement researcherImage = root.Q<VisualElement>("ResearcherImage");

            Sprite researcherSprite = GameManager.Instance.GetSaveFile().researcherContainer
            .GetResearcher(ResearcherID.Larrington).image;

            researcherImage.style.backgroundImage = new StyleBackground(researcherSprite.texture);


            root.Q<VisualElement>("SelectedPicture").style.backgroundImage = new StyleBackground(UIManager.Instance.emptyPictureSlot);


            #endregion
        }
    }
    private void UpdateSubmitButton()
    {
        bool unlocked = true;

        for (int i = 0; i < missionData.Count; i++)
        {
            if (missionData[i].savedPicture == null) unlocked = false;
        }

        if (curDescent.selectedMissions.Count != 0) unlocked = true;

        if (unlocked) root.Q<Button>("SubmitButton").pickingMode = PickingMode.Position;
        else root.Q<Button>("SubmitButton").pickingMode = PickingMode.Ignore;
    }
    #endregion

    #region Results Pop Up
    private void CalculateResults()
    {
        researchResults = new();

        for (int i = 0; i < curDescent.takenPictures.Count; i++)
        {
            for (int p = 0; p < curDescent.takenPictures[i].content.Count; p++)
            {
                if(curDescent.takenPictures[i].content[p].quality > 0)  AddScore(curDescent.takenPictures[i].content[p].creature, curDescent.takenPictures[i].content[p].quality);
            }
        }

        AddBonuses();

        for (int i = 0; i < missionData.Count; i++)
        {
            missionData[i].CalculateSuccessStatus();
        }

        UpdateFishResearch();
        UpdateMissionResults();
    }

    private void UpdateFishResearch()
    {
        if (researchResults.Count == 0) return;
        CreatureProfile curCreature = GameManager.Instance.GetSaveFile().journal.GetCreatureProfile(researchResults[curFishIndex].creature);

        root.Q<Label>("FishName").text = curCreature.GetName();

        root.Q<VisualElement>("FishImage").style.backgroundImage = new StyleBackground(curCreature.GetImage());

        root.Q<Label>("ResearchBonusAmountText").text = "+ " + researchResults[curFishIndex].bonusScore;

        ProgressBar creatureProgress = root.Q<ProgressBar>("CurrentResearchProgress");

        // If Creature isn't maxed
        if (curCreature.GetLevel() <= 2)
        {
            creatureProgress.style.display = DisplayStyle.Flex;
            root.Q<Label>("MaxedLabel").style.display = DisplayStyle.None;

            root.Q<Label>("CurLevel").text = curCreature.GetLevel().ToString();

            creatureProgress.lowValue = curCreature.GetMinValue();
            creatureProgress.highValue = curCreature.GetMaxValue();

            creatureProgress.value = curCreature.GetProgress();

            root.Q<Label>("NextLevel").text = (curCreature.GetLevel() + 1).ToString();
        }
        // If Creature is maxed
        else
        {
            root.Q<VisualElement>("ResearchProgressBar").style.display = DisplayStyle.None;
            root.Q<Label>("MaxedLabel").style.display = DisplayStyle.Flex;
        }
    }
    private void UpdateMissionResults()
    {
        if (curDescent.selectedMissions.Count != 0)
        {
            Researcher curResearcher = GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(missionData[curPopUpMissionIndex].mission.researcher);

            root.Q<Label>("MissionIndexLabel").text = (curPopUpMissionIndex + 1).ToString() + "/ " + curDescent.selectedMissions.Count.ToString();

            root.Q<VisualElement>("ResearcherPopUpImage").style.backgroundImage =
                new StyleBackground(GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(missionData[curPopUpMissionIndex].mission.researcher).image);

            if (missionData[curPopUpMissionIndex].CalculateSuccessStatus())
            {
                root.Q<Label>("MissionSuccessText").text = "Mission Successful";
                root.Q<Label>("ResearcherResponseText").text = missionData[curPopUpMissionIndex].mission.rewardText;
                root.Q<Label>("MissionRewardText").text = missionData[curPopUpMissionIndex].mission.missionRewardFlat + " Funds";

                if (curResearcher.rewardType == Researcher.RewardType.Funds)
                    root.Q<Label>("FundBonusAmount").text = "+ " +
                        ((int)(missionData[curPopUpMissionIndex].mission.missionRewardFlat * (curResearcher.rewardBonus - 1))).ToString();
                else
                    root.Q<Label>("FundBonusAmount").text = "+ " + 0;
            }
            else
            {
                root.Q<Label>("MissionSuccessText").text = "Mission Failed";
                root.Q<Label>("ResearcherResponseText").text = missionData[curPopUpMissionIndex].mission.failText;
                root.Q<Label>("MissionRewardText").text = 0 + " Funds";
                root.Q<Label>("FundBonusAmount").text = "+ " + 0;
            }
        }
        else
        {
            Researcher curResearcher = GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(ResearcherID.Larrington);

            root.Q<VisualElement>("ResearcherPopUpImage").style.backgroundImage =
                new StyleBackground(curResearcher.image);

            root.Q<Label>("MissionIndexLabel").text = " ";

            root.Q<Label>("MissionSuccessText").text = " ";
            root.Q<Label>("ResearcherResponseText").text = "What is this? Why-... *sigh*, just get back to it...";
            root.Q<Label>("MissionRewardText").text = "0 Funds";

            root.Q<Label>("FundBonusAmount").text = "+ 0";
        }
    }
    #endregion

    private void ApplyResults()
    {
        for (int i = 0; i < missionData.Count; i++)
        {
            if (missionData[i].CalculateSuccessStatus())
            {
                missionData[i].mission.completedAmount++;
                GameManager.Instance.GetSaveFile().researcherContainer.GetResearcher(missionData[i].mission.researcher).progress++;
            }
        }
        for (int i = 0; i < curDescent.takenPictures.Count; i++)
        {
            for (int p = 0; p < curDescent.takenPictures[i].content.Count; p++)
            {
                CreatureID curCreature = curDescent.takenPictures[i].content[p].creature;

                GameManager.Instance.GetSaveFile().journal.GetCreatureProfile(curCreature).progress += curDescent.takenPictures[i].content[p].quality;
            }
        }
    }
}
