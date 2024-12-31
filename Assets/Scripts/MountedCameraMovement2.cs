using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountedCameraMovement2 : MonoBehaviour
{
    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f; // Speed of horizontal and vertical rotation

    [Header("Zoom Control Settings")]
    [SerializeField] private float zoomSpeed = 10.0f; // Base speed of zooming
    [SerializeField] private float minZoom = 20.0f;   // Minimum field of view for zoom (most zoomed in)
    [SerializeField] private float maxZoom = 60.0f;   // Maximum field of view for zoom (most zoomed out)
    [SerializeField] private float zoomSmoothTime = 0.2f; // Time to interpolate towards target zoom

    private float targetFOV;
    private float zoomVelocity = 0.0f; // For SmoothDamp

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Initialize target FOV with the current FOV of the camera
        targetFOV = cam.fieldOfView;

        // Keep the cursor visible at all times
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    private void HandleRotation()
    {
        // Horizontal rotation with A and D keys
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World); // Rotate left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World); // Rotate right
        }

        // Vertical rotation with W and S keys
        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime, Space.Self); // Pitch up
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self); // Pitch down
        }
    }

    private void HandleZoom()
    {
        // Get scroll wheel input for zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // Adjust target FOV based on scroll input
            float dynamicZoomSpeed = zoomSpeed * Mathf.Lerp(0.2f, 1.0f, (cam.fieldOfView - minZoom) / (maxZoom - minZoom)); // Dynamic zoom scaling
            targetFOV -= scrollInput * dynamicZoomSpeed;
            targetFOV = Mathf.Clamp(targetFOV, minZoom, maxZoom);
        }

        // Smoothly interpolate the camera's FOV towards the target FOV
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref zoomVelocity, zoomSmoothTime);
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
