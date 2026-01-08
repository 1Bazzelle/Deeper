using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private VisualElement root;

    [SerializeField] private VisualTreeAsset mainMenu;
    [SerializeField] private VisualTreeAsset mountedCamera;
    [SerializeField] private VisualTreeAsset missionCompletion;
    [SerializeField] private VisualTreeAsset journal;
    [SerializeField] private VisualTreeAsset researchBase;

    [Header("Templates")]
    public VisualTreeAsset missionTemplate;
    public VisualTreeAsset pictureSlotTemplate;
    public VisualTreeAsset creatureProfileTemplate;

    [Header("Sprites")]
    public Sprite emptyPictureSlot;

    [HideInInspector]
    public ScreenID lastScreen;
    public enum ScreenID
    {
        None,
        MainMenu,
        MountedCamera,
        MissionCompletion,
        Journal,
        ResearchBase
    }

    private IScreen curScreen;
    private ScreenID curScreenID;

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

        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Update()
    {
        curScreen?.Update();
    }

    public void ChangeScreen(ScreenID newScreen)
    {
        root.Clear();

        switch (newScreen)
        {
            case ScreenID.MainMenu:

                lastScreen = curScreenID;
                curScreen = new MainMenuScreen();
                curScreen.Initialize(mainMenu, root);
                break;
            case ScreenID.MountedCamera:

                lastScreen = curScreenID;
                curScreenID = newScreen;
                curScreen = new MountedCameraScreen();
                curScreen.Initialize(mountedCamera, root);

                break;
            case ScreenID.MissionCompletion:

                lastScreen = curScreenID;
                curScreenID = newScreen;
                curScreen = new MissionCompletionScreen();
                curScreen.Initialize(missionCompletion, root);

                break;
            case ScreenID.Journal:

                lastScreen = curScreenID;
                curScreenID = newScreen;
                curScreen = new JournalScreen();
                curScreen.Initialize(journal, root);

                break;

            case ScreenID.ResearchBase:

                lastScreen = curScreenID;
                curScreenID = newScreen;
                curScreen = new ResearchBaseScreen();
                curScreen.Initialize(researchBase, root);

                break;
            default:

                lastScreen = curScreenID;
                curScreenID = newScreen;
                curScreen = null;

                break;
        }
    }
    public ScreenID GetCurScreen()
    {
        return curScreenID;
    }
}