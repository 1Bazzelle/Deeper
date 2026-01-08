using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;


public abstract class GameState
{
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}

public struct Descent
{
    public List<Mission> selectedMissions;
    public List<Picture> takenPictures;

    public void Setup()
    {
        selectedMissions = new();
        takenPictures = new();
    }
}

public struct PlayerStats
{
    public int pictureCapacity;
    public int shownMissionAmount;

    public void Update()
    {
        pictureCapacity = 5;
        shownMissionAmount = 2;
    }
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool inTestSpace;

    private Settings settings;

    [SerializeField] private SaveFile saveFile;

    private GameState curState;

    public Descent descent;

    [HideInInspector]
    public bool timeStopped = false;

    [HideInInspector] public UnityEvent<Sprite> pictureTaken;

    private Picture curPicture;

    public PlayerStats playerStats;

    public SaveFile GetSaveFile()
    {
        return saveFile;
    }
    public void ResetSaveFile()
    {
        saveFile.ResetData();
    }


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

        playerStats.Update();
    }

    private void Start()
    {
        if(!inTestSpace) ChangeGameState(new StateMainMenu());
        else
        {
            ChangeGameState(new StateSubmerged());
        }
    }

    private void Update()
    {
        curState?.UpdateState();
    }
    public void ChangeGameState(GameState newState)
    {
        curState?.ExitState();
        curState = newState;
        curState.EnterState();
    }

    #region Settings
    public Settings GetSettings()
    {
        return settings;
    }
    public void SetSettings(Settings newSettings)
    {
        settings = newSettings;
    }
    #endregion

    #region PictureTaking
    public void TakePicture(Camera cam, List<(CreatureID creature, int quality, float percentage, float averageRaycastValue)> hitObjects, ZoneID zone)
    {
        List<(CreatureID creature, int quality)> pictureData = new();

        for (int i = 0; i < hitObjects.Count; i++)
        {
            (CreatureID creature, int quality) curData;

            curData.creature = hitObjects[i].creature;
            curData.quality = hitObjects[i].quality;

            pictureData.Add(curData);
        }

        curPicture = new(RenderPicture(cam), pictureData, zone);

        ToggleTimeStopped(true);

        // For UI MountedCameraScreen
        pictureTaken.Invoke(curPicture.sprite);

        #region Voice Line Triggering
        CreatureID chosenCreature = CreatureID.None;
        int highestQuality = 0;
        float highestPercentage = 0;
        float highestRaycastValue = 0;

        for (int i = 0; i < hitObjects.Count; i++)
        {
            (CreatureID creature, int quality, float percentage, float averageRaycastValue) obj = hitObjects[i];

            // Quality must at least be 1, else we skip
            if (obj.quality <= 1)
                continue;
            // Prioritize highest quality, then highest percentage
            if (obj.averageRaycastValue > highestRaycastValue
                || (obj.averageRaycastValue == highestRaycastValue && obj.percentage > highestPercentage)
                )
            {
                highestPercentage = obj.percentage;
                highestRaycastValue = obj.averageRaycastValue;
                chosenCreature = obj.creature;
            }
            /*
            // Prioritize highest quality, then highest percentage
            if (obj.quality > highestQuality 
                || (obj.quality == highestQuality && obj.averageRaycastValue > highestRaycastValue) 
                || (obj.quality == highestQuality && obj.percentage > highestPercentage)
                )
            {
                highestQuality = obj.quality;
                highestPercentage = obj.percentage;
                highestRaycastValue = obj.averageRaycastValue;
                chosenCreature = obj.creature;
            }
            */
        }
        if (chosenCreature != CreatureID.None)
        {
            SoundManager.Instance.PlayVoiceLine(chosenCreature);
        }
        #endregion
    }
    private Sprite RenderPicture(Camera cam)
    {
        int imageWidth = 1920;
        int imageHeight = 1080;

        // Step 1: Create a new RenderTexture
        RenderTexture rt = new(imageWidth, imageHeight, 24);
        cam.targetTexture = rt;

        // Step 2: Render the camera's view
        RenderTexture.active = rt;

        float initFOV = cam.fieldOfView;
        cam.fieldOfView = initFOV / 1.05f;
        cam.Render(); // THIS IS WHERE THE PICTURE IS RENDERED RAAAAAHH
        cam.fieldOfView = initFOV;

        // Step 3: Create a new Texture2D and read the RenderTexture into it
        Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        screenShot.Apply();

        // Step 4: Reset the active RenderTexture and the camera's target texture
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt); // Cleanup

        // Step 5: Convert the Texture2D to PNG format
        byte[] imageBytes = screenShot.EncodeToPNG();

        // Step 6: Save the image to persistent storage
        string filePath = Path.Combine(Application.persistentDataPath, "CameraPicture.png");
        File.WriteAllBytes(filePath, imageBytes);

        //Debug.Log($"Picture saved to: {filePath}");

        imageBytes = File.ReadAllBytes(filePath);

        // Step 3: Create a new Texture2D and load the image data into it
        Texture2D texture = new Texture2D(1920, 1080); // Initialize with any size, will resize automatically
        texture.LoadImage(imageBytes); // Load the image data

        // Step 4: Return the Sprite from the Texture2D
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    public void SaveCurrentPicture(bool yes)
    {
        if (yes)
        {
            descent.takenPictures.Add(curPicture);

            /*
            Debug.Log("Saving Picture with: ");
            foreach((CreatureID creature, int quality) picture in curPicture.content)
            {
                Debug.Log("CreatureID: " + picture.creature);
                Debug.Log("Quality: " + picture.quality); 
            }
            */
        }
        ToggleTimeStopped(false);
    }
    #endregion

    public void GenerateMissions()
    {
        saveFile.missionPool.UpdateAvailableMissions();
        List<Mission> availableMissions = saveFile.missionPool.GetAvailableMissions();

        int missionCount = playerStats.shownMissionAmount;

        saveFile.currentMissionSelection.Clear();

        for (int i = 0; i < missionCount; i++)
        {
            if (availableMissions.Count == 0)
            {
                Debug.LogError("no available missions");
                break; // Exit the loop if there are no missions to select from.
            }

            int rand = Random.Range(0, availableMissions.Count);

            // Check if duplicates are allowed (based on the count of availableMissions)
            if (saveFile.currentMissionSelection.Contains(availableMissions[rand]))
            {
                // Only consider missions, if there are enough available missions to fill the unlocked mission amount
                if (availableMissions.Count >= missionCount)
                {
                    // Skip duplicate and retry if we still have enough unique missions available.
                    i--;
                    continue;
                }
            }

            Mission curMission = availableMissions[rand];

            // If the needed Creature for the Mission is 'None' and there is only one element in the list,
            // consider it a generic one
            if(curMission.creatureNeeded.Count == 1 && curMission.creatureNeeded[0] == CreatureID.None)
            {
                curMission = curMission.GenerateGenericMission();
            }
            // Add the randomly selected mission.
            saveFile.currentMissionSelection.Add(curMission);
        }
    }
    public void ClearMissions()
    {
        saveFile.currentMissionSelection.Clear();
    }

    public void ToggleTimeStopped(bool toggle)
    {
        if(toggle)
        {
            PlayerManager.Instance.GetPlayer().silenced = true;
            Time.timeScale = 0;

            timeStopped = true;
        }
        else
        {
            Time.timeScale = 1;

            timeStopped = false;
            PlayerManager.Instance.GetPlayer().silenced = false;
        }
    }

    #region Debugging
    public void DisplayDebugSphere(Transform transform, Color color, float scale)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.position = transform.position;
        // Can't just make 'false' because the scale will reset to 1,1,1
        sphere.transform.SetParent(transform, true);
        Destroy(sphere.GetComponent<SphereCollider>());
        Destroy(sphere, 0.025f);
    }
    public void DisplayDebugSphereWorldSpace(Vector3 worldPosition, Color color, float scale)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.position = worldPosition;
        Destroy(sphere.GetComponent<SphereCollider>());
        Destroy(sphere, 0.025f);
    }

    #endregion
}
