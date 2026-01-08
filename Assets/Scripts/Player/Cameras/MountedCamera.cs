using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MountedCamera
{
    [System.Serializable]
    public struct MountedCameraData
    {
        [Header("Rotation Settings")]
        public float rotationSpeed; // Speed of horizontal and vertical rotation

        [Header("Zoom Settings")]
        public float zoomSpeed; // Base speed of zooming
        public float minZoom;   // Minimum field of view for zoom (most zoomed in)
        public float maxZoom;   // Maximum field of view for zoom (most zoomed out)
        public float zoomSmoothTime; // Time to interpolate towards target zoom

        [Header("Sound")]
        public float moveAudioVolume;
        public float zoomAudioVolume;

        [Header("Raycast Parameters")]
        public bool showGizmos;
        public int xRays; public int yRays;
        public float range;
        public float minValue;
        public float fallOffStart;
        public float fallOffStrength;
    }

    private Player player;

    private MountedCameraData camData;
    private Camera cam;

    private float initFOV;
    private float targetFOV;
    private float zoomVelocity = 0.0f; // For SmoothDamp


    private float fadeSpeed = 0.5f;

    private AudioSource moveSource;
    private AudioSource zoomSource;
    private AudioSource takePictureSource;

    public void Initialize(Player player, MountedCameraData camData, Camera cam, AudioSource _moveSource, AudioSource _zoomSource, AudioSource _takePictureSource)
    {
        this.player = player;

        this.camData = camData;
        this.cam = cam;

        initFOV = cam.fieldOfView;
        targetFOV = initFOV;

        moveSource = _moveSource;
        zoomSource = _zoomSource;
        takePictureSource = _takePictureSource;
    }

    public void UpdateCamera()
    {
        if (!player.silenced)
        {
            HandleRotation();
            HandleZoom();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !GameManager.Instance.timeStopped)
        {
            takePictureSource.Play();

            // GameManager.Instance.TakePicture(cam, DetermineCreatureQuality(cam, 25, 25, 10, 100, 0, 0.314f, 0.072f), PlayerManager.Instance.GetCurZone());
            GameManager.Instance.TakePicture(cam, DetermineCreatureQuality(cam, camData.xRays, camData.yRays, 0, camData.range, camData.minValue, camData.fallOffStart, camData.fallOffStrength), PlayerManager.Instance.GetCurZone());

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleRotation()
    {
        float pitchLimit = 90f; // Maximum pitch limit in degrees

        Vector3 currentRotation = cam.transform.localEulerAngles;

        float currentPitch = currentRotation.x;
        if (currentPitch > 180f) currentPitch -= 360f;

        // horizontal
        if (Input.GetKey(KeyCode.A))
        {
            cam.transform.Rotate(Vector3.up, -camData.rotationSpeed * Time.deltaTime, Space.World); // Rotate left
            FadeInAudio(moveSource, camData.moveAudioVolume);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            cam.transform.Rotate(Vector3.up, camData.rotationSpeed * Time.deltaTime, Space.World); // Rotate right
            FadeInAudio(moveSource, camData.moveAudioVolume);
        }

        float pitchChange = 0f;

        // vertical
        if (Input.GetKey(KeyCode.W))
        {
            pitchChange = -camData.rotationSpeed * Time.deltaTime; // Pitch up
            FadeInAudio(moveSource, camData.moveAudioVolume);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            pitchChange = camData.rotationSpeed * Time.deltaTime; // Pitch down
            FadeInAudio(moveSource, camData.moveAudioVolume);
        }

        if(!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            FadeOutAudio(moveSource);
        }

        float newPitch = Mathf.Clamp(currentPitch + pitchChange, -pitchLimit, pitchLimit);

        currentRotation.x = newPitch;
        cam.transform.localEulerAngles = new Vector3(currentRotation.x, cam.transform.localEulerAngles.y, 0f);
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // Set target FOV depending on scroll input
            float dynamicZoomSpeed = camData.zoomSpeed * Mathf.Lerp(0.2f, 1.0f, (cam.fieldOfView - camData.minZoom) / (camData.maxZoom - camData.minZoom)); // Dynamic zoom scaling
            targetFOV -= scrollInput * dynamicZoomSpeed;
            targetFOV = Mathf.Clamp(targetFOV, camData.minZoom, camData.maxZoom);
        }

        // Smoothly interpolate FOV toward target FOV
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref zoomVelocity, camData.zoomSmoothTime);

        if (Mathf.Abs(cam.fieldOfView - targetFOV) > 0.5f) FadeInAudio(zoomSource, camData.zoomAudioVolume);
        else FadeOutAudio(zoomSource);
    }

    public void ResetCamera()
    {
        cam.transform.rotation = PlayerManager.Instance.GetPlayerTransform().rotation;
        targetFOV = initFOV;
        cam.fieldOfView = initFOV;
    }

    private struct RaycastData
    {
        public Creature creatureScript;
        public CreatureID creature;
        public int raycastHits;
        public float raycastValue;
    }
    private List<(CreatureID creature, int quality, float percentage, float averageRaycastValue)> DetermineCreatureQuality(Camera cam, int xRays, int yRays, float minRange, float range, float minValue, float fallOffStart, float fallOffStrength)
    {
        List<RaycastData> hitCreatures = new();

        float centralMaxX = 1.0f - fallOffStart;
        float centralMaxY = 1.0f - fallOffStart;

        // Iterate over every single ray
        for (int x = 0; x < xRays; x++)
        {
            for (int y = 0; y < yRays; y++)
            {
                float normalizedX = x / (float)(xRays - 1);
                float normalizedY = y / (float)(yRays - 1);

                Vector3 viewportPoint = new Vector3(normalizedX, normalizedY, cam.nearClipPlane);
                Vector3 worldPosition = cam.ViewportToWorldPoint(viewportPoint);

                Vector3 dir = (worldPosition - cam.transform.position).normalized;

                float distanceToCenterX = Mathf.Max(0, Mathf.Abs(normalizedX - 0.5f) - (centralMaxX - 0.5f));
                float distanceToCenterY = Mathf.Max(0, Mathf.Abs(normalizedY - 0.5f) - (centralMaxY - 0.5f));

                float combinedFallOffRatio = Mathf.Max(distanceToCenterX, distanceToCenterY) / (0.5f - fallOffStart);
                combinedFallOffRatio = Mathf.Clamp01(combinedFallOffRatio);

                float scaledFallOffStrength = Mathf.Lerp(0.01f, 10.0f, fallOffStrength);
                float easedFallOff = Mathf.Pow(combinedFallOffRatio, scaledFallOffStrength);

                float rayValue = Mathf.Lerp(1, minValue, easedFallOff);

                int layerMask = Constants.LAYER_CREATURE | Constants.LAYER_TERRAIN | Constants.LAYER_SUBMARINE;

                // Has the Raycast hit a creature? This will save its data into the List
                if (Physics.Raycast(worldPosition, dir, out RaycastHit hit, range, layerMask))
                {
                    int hitLayerMask = 1 << hit.collider.gameObject.layer;

                    if ((hitLayerMask & (Constants.LAYER_SUBMARINE | Constants.LAYER_TERRAIN)) != 0)
                    {
                        // Debug.Log("I am a raycast. I just hit a submarine or a rock. I am sad. I am a sad raycast. boohoo");
                        continue;
                    }
                        

                    if (!hit.collider.gameObject.TryGetComponent<Creature>(out var hitCreature)) 
                        Debug.LogError("No CreatureScript on hit Creature: " + hit.collider.gameObject.name);

                    bool alreadyExisted = false;

                    // Look through the list of creatures if Creature is already in there
                    for (int i = 0; i < hitCreatures.Count; i++)
                    {
                        // creature with same CreatureID was found in hitCreatures
                        if (hitCreatures[i].creature == hitCreature.id)
                        {
                            RaycastData temp = hitCreatures[i];

                            temp.creatureScript = hitCreature;

                            temp.creature = hitCreature.id;

                            temp.raycastValue += rayValue;
                            temp.raycastHits += 1;

                            hitCreatures[i] = temp;

                            alreadyExisted = true;
                            break;
                        }
                    }

                    // If the Creature is new to the List, add an element holding its data
                    if (!alreadyExisted)
                    {
                        RaycastData temp = new();

                        temp.creatureScript = hitCreature;

                        temp.creature = hitCreature.id;

                        temp.raycastValue = rayValue;
                        temp.raycastHits = 1;

                        hitCreatures.Add(temp);
                    }
                }
            }
        }

        int count = hitCreatures.Count;

        List<(CreatureID creature, int quality, float percentage, float averageRaycastValue)> pictureEvaluation = new();

        // Extract Quality Data from every hit creature
        for (int i = 0; i < count; i++)
        {
            (CreatureID creature, int quality, float percentage, float averageRaycastValue) curPicture = new();

            curPicture.creature = hitCreatures[i].creature;

            Debug.Log("Hit Creature: " + curPicture.creature);

            float raycastHitPercentage = (float) hitCreatures[i].raycastHits / (xRays * yRays);

            Debug.Log("Percentage: " + raycastHitPercentage * 100);

            curPicture.percentage = raycastHitPercentage;

            int starRating = hitCreatures[i].creatureScript.GetStarRating(raycastHitPercentage);

            float averageRayCastValue = hitCreatures[i].raycastValue / hitCreatures[i].raycastHits;

            curPicture.averageRaycastValue = averageRayCastValue;

            Debug.Log("Average Raycast Value: " + averageRayCastValue + " (minimum: " + hitCreatures[i].creatureScript.GetMinAverageRaycastValue() +  ")");

            if (averageRayCastValue < hitCreatures[i].creatureScript.GetMinAverageRaycastValue())
            {
                starRating = Mathf.Clamp(starRating - 1, 1, 3);
            }

            Debug.Log("FINAL STARS: " + starRating);
            curPicture.quality = starRating;

            // Only add the picture, if it has at least 1 Star
            if (starRating >= 1) pictureEvaluation.Add(curPicture);
        }

        return pictureEvaluation;
    }

    private void FadeOutAudio(AudioSource source)
    {
        if (source.isPlaying == true)
        {
            source.volume -= Time.deltaTime / fadeSpeed;
            if (source.volume <= 0)
            {
                source.Stop();
            }
        }
    }
    private void FadeInAudio(AudioSource source, float maxVolume)
    {
        if (source.isPlaying == false) source.Play();

        source.volume += Time.deltaTime / fadeSpeed;
        if (source.volume >= maxVolume) source.volume = maxVolume;
    }

    public void OnDrawGizmos()
    {
        if (!camData.showGizmos) return;

        if (cam == null) return;

        if (camData.xRays <= 1 || camData.yRays <= 1)
        {
            // Amounts must be greater than 1
            return;
        }

        float centralMaxX = 1.0f - camData.fallOffStart;
        float centralMaxY = 1.0f - camData.fallOffStart;

        for (int x = 0; x < camData.xRays; x++)
        {
            for (int y = 0; y < camData.yRays; y++)
            {
                float normalizedX = x / (float)(camData.xRays - 1);
                float normalizedY = y / (float)(camData.yRays - 1);

                Vector3 viewportPoint = new Vector3(normalizedX, normalizedY, cam.nearClipPlane);
                Vector3 worldPosition = cam.ViewportToWorldPoint(viewportPoint);

                Vector3 dir = (worldPosition - cam.transform.position).normalized;

                // Calculate the distance from the current point to the edges of the central rectangle
                float distanceToCenterX = Mathf.Max(0, Mathf.Abs(normalizedX - 0.5f) - (centralMaxX - 0.5f));
                float distanceToCenterY = Mathf.Max(0, Mathf.Abs(normalizedY - 0.5f) - (centralMaxY - 0.5f));

                // Calculate the combined falloff ratio based on the maximum distance
                float combinedFallOffRatio = Mathf.Max(distanceToCenterX, distanceToCenterY) / (0.5f - camData.fallOffStart);
                combinedFallOffRatio = Mathf.Clamp01(combinedFallOffRatio);

                // Map fallOffStrength to an exponential scale for a wider range of falloff effects
                float scaledFallOffStrength = Mathf.Lerp(0.01f, 10.0f, camData.fallOffStrength);
                float easedFallOff = Mathf.Pow(combinedFallOffRatio, scaledFallOffStrength);

                // Interpolate the ray value between maxValue (1) and minValue based on the falloff
                float rayValue = Mathf.Lerp(1, camData.minValue, easedFallOff);

                // Interpolate color based on ray value
                Color rayColor = Color.Lerp(Color.green, Color.red, (1 - rayValue) / (1 - camData.minValue));

                // Draw the ray with the computed color
                Gizmos.color = rayColor;
                //Gizmos.DrawLine(cam.transform.position, cam.transform.position + dir * range);

                Gizmos.DrawSphere(cam.transform.position + dir * camData.range, 0.25f);
            }
        }
    }
}
