using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MountedCameraMovement : MonoBehaviour
{
    // Variables to control acceleration, max angular velocity, and deceleration
    [SerializeField] private float horizontalAcceleration = 2.0f;
    [SerializeField] private float verticalAcceleration = 2.0f;
    [SerializeField] private float maxHorizontalSpeed = 100.0f;
    [SerializeField] private float maxVerticalSpeed = 80.0f;
    [SerializeField] private float decelerationFactor = 5.0f; // How quickly the camera decelerates when the mouse stops moving

    // Zoom control variables
    [SerializeField] private float zoomSpeed = 10.0f; // Base speed of zooming
    [SerializeField] private float minZoom = 20.0f;   // Minimum field of view for zoom (most zoomed in)
    [SerializeField] private float maxZoom = 60.0f;   // Maximum field of view for zoom (most zoomed out)
    [SerializeField] private float zoomSmoothTime = 0.2f; // Time to interpolate towards target zoom

    private float horizontalSpeed = 0.0f;
    private float verticalSpeed = 0.0f;

    // Sensitivity for mouse movement
    [SerializeField] private float sensitivity = 5.0f;

    // Camera component for zoom
    private Camera cam;

    private PictureTaker pictureTaker;

    // Variables for cursor position management
    private Vector3 savedMousePosition;

    // Smooth zooming variables
    private float targetFOV;
    private float zoomVelocity = 0.0f; // For SmoothDamp

    void Start()
    {
        cam = GetComponent<Camera>();
        pictureTaker = GetComponent<PictureTaker>();

        // Initialize the current rotation with the camera's current rotation angles
        Vector3 currentRotation = transform.localEulerAngles;

        // Initialize the target FOV with the current FOV of the camera
        targetFOV = cam.fieldOfView;
    }

    void Update()
    {
        HandleCursor();

        // Only rotate the camera if the right mouse button is held down
        if (Input.GetMouseButton(1))
        {
            // Get current mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Check if mouse is moving horizontally
            if (Mathf.Abs(mouseX) > 0.01f) // Small threshold to avoid jittering
            {
                // Accelerate the horizontal speed based on mouse movement
                horizontalSpeed += mouseX * horizontalAcceleration * sensitivity;
            }
            else
            {
                // Decelerate the horizontal speed if the mouse stops moving
                horizontalSpeed = Mathf.Lerp(horizontalSpeed, 0, decelerationFactor * Time.deltaTime);
            }

            // Check if mouse is moving vertically
            if (Mathf.Abs(mouseY) > 0.01f)
            {
                // Accelerate the vertical speed based on mouse movement (inverted Y)
                verticalSpeed += -mouseY * verticalAcceleration * sensitivity;
            }
            else
            {
                // Decelerate the vertical speed if the mouse stops moving
                verticalSpeed = Mathf.Lerp(verticalSpeed, 0, decelerationFactor * Time.deltaTime);
            }

            // Clamp the speeds to the max allowed values
            horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxHorizontalSpeed, maxHorizontalSpeed);
            verticalSpeed = Mathf.Clamp(verticalSpeed, -maxVerticalSpeed, maxVerticalSpeed);
        }
        else
        {
            // Decelerate the camera when the right mouse button is released
            horizontalSpeed = Mathf.Lerp(horizontalSpeed, 0, decelerationFactor * Time.deltaTime);
            verticalSpeed = Mathf.Lerp(verticalSpeed, 0, decelerationFactor * Time.deltaTime);
        }

        // Apply the rotation to the camera (only rotate on the local axes)
        transform.localEulerAngles += new Vector3(verticalSpeed * Time.deltaTime, horizontalSpeed * Time.deltaTime, 0.0f);

        // Zoom functionality
        HandleZoom();

        if(Input.GetKeyDown(KeyCode.Space))
            TakePicture();
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        transform.localRotation = Quaternion.Euler(0,0,0);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void TakePicture()
    {
        pictureTaker.TakePicture();
    }
    private void HandleZoom()
    {
        // Get the scroll wheel input
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
    private void HandleCursor()
    {
        // When the right mouse button is pressed
        if (Input.GetMouseButtonDown(1))
        {
            // Save the current mouse position
            savedMousePosition = Input.mousePosition;

            // Lock the cursor to the center of the screen and hide it
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // When the right mouse button is released
        if (Input.GetMouseButtonUp(1))
        {
            // Restore the mouse cursor position
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Reposition the cursor to the saved position
            Cursor.SetCursor(null, savedMousePosition, CursorMode.Auto);
        }
    }
}
