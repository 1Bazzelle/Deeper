using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class CockpitViewCameraMovement : MonoBehaviour
{
    [SerializeField] private Transform followTransform;

    // Variables to control acceleration, max angular velocity, and deceleration
    [SerializeField] private float horizontalAcceleration = 2.0f;
    [SerializeField] private float verticalAcceleration = 2.0f;
    [SerializeField] private float maxHorizontalSpeed = 100.0f;
    [SerializeField] private float maxVerticalSpeed = 80.0f;
    [SerializeField] private float decelerationFactor = 5.0f; // How quickly the camera decelerates when the mouse stops moving

    private float horizontalSpeed = 0.0f;
    private float verticalSpeed = 0.0f;

    // Sensitivity for mouse movement
    [SerializeField] private float sensitivity = 5.0f;

    // Boundaries for vertical rotation
    [SerializeField] private float maxUpRotation = 40.0f;     // Maximum vertical rotation upward (in degrees)
    [SerializeField] private float maxDownRotation = 10.0f;   // Maximum vertical rotation downward (in degrees)

    private Vector3 initialRotation;

    void Start()
    {
        // Save the initial rotation at the start
        initialRotation = transform.localEulerAngles;

        // Lock the cursor and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = new Vector3(followTransform.position.x, followTransform.position.y, followTransform.position.z);

        HandleCursorLock();

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

        // Calculate the desired new rotation angles
        Vector3 newRotation = transform.localEulerAngles + new Vector3(verticalSpeed * Time.deltaTime, horizontalSpeed * Time.deltaTime, 0.0f);

        // Calculate relative rotation compared to the initial rotation
        float relativeVerticalRotation = Mathf.DeltaAngle(initialRotation.x, newRotation.x);

        // Vertical boundary behavior: decelerate smoothly near boundaries
        if (relativeVerticalRotation > maxUpRotation)
        {
            // If we're trying to move further upwards, smoothly decelerate
            if (verticalSpeed > 0)
            {
                verticalSpeed = Mathf.Lerp(verticalSpeed, 0, decelerationFactor * Time.deltaTime);
            }
            newRotation.x = initialRotation.x + maxUpRotation;
        }
        else if (relativeVerticalRotation < -maxDownRotation)
        {
            // If we're trying to move further downwards, smoothly decelerate
            if (verticalSpeed < 0)
            {
                verticalSpeed = Mathf.Lerp(verticalSpeed, 0, decelerationFactor * Time.deltaTime);
            }
            newRotation.x = initialRotation.x - maxDownRotation;
        }

        // Apply the updated rotation without clamping the horizontal rotation
        transform.localEulerAngles = newRotation;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void HandleCursorLock()
    {
        // Lock the cursor and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}