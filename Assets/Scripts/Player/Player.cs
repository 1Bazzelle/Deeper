using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;


public class Player : MonoBehaviour 
{
    #region Cameras
    [Header("Cameras")]
    [SerializeField] private Camera cockpitCamera;
    [SerializeField] private Transform followTransform;
    [SerializeField] private CockpitCamera cockpitCamScript;
    [SerializeField] private CockpitCamera.CockpitCameraData cockpitCamData;

    [Header("")]
    [SerializeField] private Camera mountedCamera;
    [SerializeField] private MountedCamera mountedCamScript;
    [SerializeField] private MountedCamera.MountedCameraData mountedCamData;
    #endregion

    [Header("")]
    [SerializeField] private bool allowHideSubmarine;
    [SerializeField] private List<GameObject> thingsToHide;

    #region Submarine
    [Header("Submarine")]
    [SerializeField] private SubmarineMovement.MovementData subMovementData;

    private Rigidbody rb;

    private SubmarineMovement subMovement;

    #endregion

    #region Sound

    public bool silenced;

    [Header("Sound")]
    [SerializeField] private AudioSource moveAudio;
    [SerializeField] private float maxMoveAudioVolume = 0.1f;
    private float fadeSpeed = 4;

    [SerializeField] private AudioSource moveMountedCameraAudio;
    [SerializeField] private AudioSource zoomMountedCameraAudio;
    [SerializeField] private AudioSource switchToMountedCameraAudio;
    [SerializeField] private AudioSource takePictureAudio;

    #endregion

    public enum PlayerMode
    {
        None,
        Cockpit,
        MountedCamera
    }

    private PlayerMode curMode;

    private void Awake()
    {
        subMovement = new();
        
        cockpitCamScript = new();

        mountedCamScript = new();

        curMode = PlayerMode.Cockpit;
    }

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();

        subMovement.Initialize(subMovementData, rb, cockpitCamera.transform);
        cockpitCamScript.Initialize(cockpitCamData, cockpitCamera, followTransform);
        mountedCamScript.Initialize(this, mountedCamData, mountedCamera, moveMountedCameraAudio, zoomMountedCameraAudio, takePictureAudio);
    }

    private void Update()
    {
        if (silenced)
        {
            moveAudio.Stop();
            moveMountedCameraAudio.Stop();
            zoomMountedCameraAudio.Stop();
        }
        #region Sound

        // Debug.Log(rb.linearVelocity.magnitude); // Max: 1.66f // I made it 2 because you can move faster when combining axes

        if (!silenced)
        { 
            if (rb.linearVelocity.magnitude <= 0.075f) FadeOutAudio(moveAudio);
            else
            {
                FadeInAudio(moveAudio, maxMoveAudioVolume);
                moveAudio.pitch = 1 + (rb.linearVelocity.magnitude / 2f) * 0.45f;
            }
        }
        #endregion

        #region Mounted Camera Switching
            if (curMode == PlayerMode.Cockpit)
        {
            cockpitCamScript.UpdateCamera();
            if(!silenced) subMovement.UpdateMovement();
        }
        else if(curMode == PlayerMode.MountedCamera)
        {
            mountedCamScript.UpdateCamera();
            subMovement.Decelerate();
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            // From Cockpit to Mounted
            if (curMode == PlayerMode.Cockpit)
            {
                SwitchToMountedCamera();
            }
            // From Mounted to Cockpit
            else if (curMode == PlayerMode.MountedCamera)
            {
                SwitchToCockpitCamera();
            }
        }
        #endregion

        AdaptFogColor();

        if (Input.GetKeyDown(KeyCode.Tab) && UIManager.Instance.GetCurScreen() != UIManager.ScreenID.Journal)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            GameManager.Instance.ToggleTimeStopped(true);
            UIManager.Instance.ChangeScreen(UIManager.ScreenID.Journal);
        }

        if (curMode == PlayerMode.Cockpit && allowHideSubmarine && Input.GetKeyDown(KeyCode.RightShift))
        {
            if (thingsToHide[0].activeInHierarchy)
            {
                foreach (GameObject thing in thingsToHide)
                {
                    thing.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject thing in thingsToHide)
                {
                    thing.SetActive(true);
                }
            }
        }
    }

    private void AdaptFogColor()
    {
        /*
        if (transform.position.y < 0 && transform.position.y > -Constants.maxDepth)
        {
            // RenderSettings.fogDensity = Mathf.Lerp(Constants.minFogDensity, Constants.maxFogDensity, Mathf.Clamp(Mathf.Abs(transform.position.y) / Mathf.Abs(Constants.maxDepth), 0, 1));
            for (int i = 0; i + 1 < Constants.depthColors.Length; i++)
            {

                if (transform.position.y < -Constants.depthColors[i].depth && transform.position.y > -Constants.depthColors[i + 1].depth)
                {
                    //Debug.Log("newColor: " + Constants.depthColors[i + 1].color);

                    // RenderSettings.fogColor = Constants.depthColors[i].color;
                }
            }
        }
        */
    }

    public void SetMode(PlayerMode newMode)
    {
        if (newMode == PlayerMode.MountedCamera)
        {
            SwitchToMountedCamera();
            return;
        }
        // From Mounted to Cockpit
        else if (newMode == PlayerMode.Cockpit)
        {
            SwitchToCockpitCamera();
            return;
        }
    }

    private void SwitchToCockpitCamera()
    {
        if(UIManager.Instance != null) UIManager.Instance.ChangeScreen(UIManager.ScreenID.None);

        zoomMountedCameraAudio.Stop();
        moveMountedCameraAudio.Stop();

        curMode = PlayerMode.Cockpit;

        mountedCamera.gameObject.SetActive(false);
        cockpitCamera.gameObject.SetActive(true);
    }
    private void SwitchToMountedCamera()
    {
        if(UIManager.Instance != null) UIManager.Instance.ChangeScreen(UIManager.ScreenID.MountedCamera);

        switchToMountedCameraAudio.Play();

        curMode = PlayerMode.MountedCamera;

        mountedCamScript.ResetCamera();

        cockpitCamera.gameObject.SetActive(false);
        mountedCamera.gameObject.SetActive(true);
    }

    #region Sound
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
    #endregion
}
