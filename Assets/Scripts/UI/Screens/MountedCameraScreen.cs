using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MountedCameraScreen : IScreen
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

        GameManager.Instance.pictureTaken.AddListener(OnPictureTaken);

        UpdatePicturesLeft();

        root.Q<Label>("PictureCapacityNotifier").style.display = DisplayStyle.None;
    }
    public void Update() { }
    private void SubscribeButtons()
    {
        RegisterButton("DiscardButton", OnDiscardButtonClicked);
        RegisterButton("SaveButton", OnSavedButtonClicked);
    }

    private void RegisterButton(string buttonName, Action callback)
    {
        Button button = root.Q<Button>(buttonName);

        if (button != null)
        {
            button.clicked += () =>
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundID.Click);
                callback(); // Ensure the original button action is still called
            };

            button.RegisterCallback<PointerEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundManager.SoundID.Hover));
        }
    }

    private void OnPictureTaken(Sprite pic)
    {
        root.Q<VisualElement>("PicturePopUpContainer").style.display = DisplayStyle.Flex;

        root.Q<VisualElement>("Picture").style.backgroundImage = new StyleBackground(pic);
    }

    private void OnDiscardButtonClicked()
    {
        root.Q<VisualElement>("PicturePopUpContainer").style.display = DisplayStyle.None;

        GameManager.Instance.SaveCurrentPicture(false);

        if (GameManager.Instance.descent.takenPictures.Count >= GameManager.Instance.playerStats.pictureCapacity)
            DisplayPictureCapacityText();
    }
    private void OnSavedButtonClicked()
    {
        root.Q<VisualElement>("PicturePopUpContainer").style.display = DisplayStyle.None;

        if (GameManager.Instance.descent.takenPictures.Count < GameManager.Instance.playerStats.pictureCapacity)
        {
            GameManager.Instance.SaveCurrentPicture(true);
            UpdatePicturesLeft();
        }
        else
            GameManager.Instance.SaveCurrentPicture(false);
        
        if (GameManager.Instance.descent.takenPictures.Count >= GameManager.Instance.playerStats.pictureCapacity)
            DisplayPictureCapacityText();
    }

    private void UpdatePicturesLeft()
    {
        root.Q<Label>("PicturesLeft").text = GameManager.Instance.descent.takenPictures.Count + "/ " + GameManager.Instance.playerStats.pictureCapacity;
    }
    private void DisplayPictureCapacityText()
    {
        root.Q<Label>("PictureCapacityNotifier").style.display = DisplayStyle.Flex;
    }
}
