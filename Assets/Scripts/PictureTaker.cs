using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class PictureTaker : MonoBehaviour
{
    [SerializeField] private int imageWidth = 1920;
    [SerializeField] private int imageHeight = 1080;

    [SerializeField] private Image displayImage;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    
    void Update()
    {
        
    }

    public void TakePicture()
    {
        // Step 1: Create a new RenderTexture
        RenderTexture rt = new(imageWidth, imageHeight, 24);
        cam.targetTexture = rt;

        // Step 2: Render the camera's view
        RenderTexture.active = rt;
        cam.Render();

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

        Debug.Log($"Picture saved to: {filePath}");

        imageBytes = File.ReadAllBytes(filePath);

        // Step 3: Create a new Texture2D and load the image data into it
        Texture2D texture = new Texture2D(1920, 1080); // Initialize with any size, will resize automatically
        texture.LoadImage(imageBytes); // Load the image data

        // Step 4: Create a Sprite from the Texture2D
        Sprite loadedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Step 5: Set the sprite to a UI Image component (or use it as needed)
        displayImage.sprite = loadedSprite;
    }
}
