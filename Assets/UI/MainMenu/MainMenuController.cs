using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private VisualElement ui;

    private Button continueButton;
    private Button newGameButton;
    private Button settingsButton;
    private Button exitButton;


    private VisualElement newGameConfirmationContainer;
    private Button newGameCancelButton;
    private Button newGameConfirmButton;
    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        #region Main Menu Buttons
        continueButton = ui.Q<Button>("ContinueButton");
        newGameButton = ui.Q<Button>("NewGameButton");
        settingsButton = ui.Q<Button>("SettingsButton");
        exitButton = ui.Q<Button>("ExitButton");

        continueButton.clicked += ContinueButtonClicked;
        newGameButton.clicked += NewGameButtonClicked;
        settingsButton.clicked += SettingsButtonClicked;
        exitButton.clicked += ExitButtonClicked;
        #endregion

        #region New Game Confirmation PopUp
        newGameConfirmationContainer = ui.Q<VisualElement>("NewGameConfirmationContainer");
        newGameConfirmationContainer.style.display = DisplayStyle.None;

        newGameCancelButton = ui.Q<Button>("NewGameCancelButton");
        newGameCancelButton.clicked += NewGameCancelButtonClicked;
        newGameConfirmButton = ui.Q<Button>("NewGameConfirmButton");
        newGameConfirmButton.clicked += NewGameConfirmButtonClicked;


        #endregion
    }

    private void ContinueButtonClicked()
    {

    }
    private void NewGameButtonClicked()
    {
        
    }
    private void SettingsButtonClicked()
    {

    }
    private void ExitButtonClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    private void NewGameCancelButtonClicked()
    {

    }
    private void NewGameConfirmButtonClicked()
    {

    }
}
