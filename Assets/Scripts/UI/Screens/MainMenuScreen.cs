using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuScreen : IScreen
{
    private VisualTreeAsset tree;
    private VisualElement root;

    private VisualElement screenElements;
    public void Initialize(VisualTreeAsset tree, VisualElement root)
    {
        this.tree = tree;

        screenElements = tree.Instantiate();
        root.Add(screenElements);

        this.root = root;



        SubscribeButtons();
    }
    public void Update() { }

    private void SubscribeButtons()
    {
        RegisterButton("ContinueButton", OnContinueButtonClick);
        RegisterButton("NewGameButton", OnNewGameButtonClick);
        RegisterButton("SettingsButton", OnSettingsButtonClick);
        RegisterButton("ExitButton", OnExitButtonClick);
        RegisterButton("ExitSettingsButton", OnExitSettingsButtonClick);
        RegisterButton("ApplyChangesButton", OnApplyChangesButtonClick);
        RegisterButton("NewGameConfirmButton", OnNewGameConfirmButtonClick);
        RegisterButton("NewGameCancelButton", OnNewGameCancelButtonClick);
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
            button.RegisterCallback<PointerEnterEvent>(evt => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));
        }
    }
    // This is not done, this is wrong, it does something completely different
    private void OnContinueButtonClick()
    {
        OnNewGameConfirmButtonClick();
        //GameManager.Instance.ChangeGameState(new StateResearchBase());
    }
    private void OnNewGameButtonClick()
    {
        root.Q<VisualElement>("NewGameConfirmationContainer").style.display = DisplayStyle.Flex;
    }
    private void OnSettingsButtonClick()
    {
        root.Q<VisualElement>("NewGameConfirmationContainer").style.display = DisplayStyle.None;

        Settings settings = GameManager.Instance.GetSettings();

        root.Q<Slider>("SensitivitySlider").value = settings.sensitivity;

        root.Q<Slider>("MasterVolumeSlider").value = settings.masterVolume;
        root.Q<Slider>("MusicVolumeSlider").value = settings.musicVolume;
        root.Q<Slider>("EffectsVolumeSlider").value = settings.effectVolume;


        root.Q<VisualElement>("SettingsContainer").style.display = DisplayStyle.Flex;
    } // DONE
    private void OnExitButtonClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    } // DONE

    #region Settings Menu

    private void OnExitSettingsButtonClick()
    {
        root.Q<VisualElement>("SettingsContainer").style.display = DisplayStyle.None;
    }
    private void OnApplyChangesButtonClick()
    {
        Settings newSettings = new();

        newSettings.sensitivity = root.Q<Slider>("SensitivitySlider").value;

        newSettings.masterVolume = root.Q<Slider>("MasterVolumeSlider").value;
        newSettings.musicVolume = root.Q<Slider>("MusicVolumeSlider").value;
        newSettings.effectVolume = root.Q<Slider>("EffectsVolumeSlider").value;

        GameManager.Instance.SetSettings(newSettings);
    }

    #endregion

    #region ConfirmationPopUp
    private void OnNewGameConfirmButtonClick()
    {
        GameManager.Instance.ResetSaveFile();

        GameManager.Instance.ChangeGameState(new StateResearchBase());
    }
    private void OnNewGameCancelButtonClick()
    {
        root.Q<VisualElement>("NewGameConfirmationContainer").style.display = DisplayStyle.None;
    }
    #endregion
}
