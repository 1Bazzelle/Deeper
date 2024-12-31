using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(InterpolateRotation))]
public class Submarine : MonoBehaviour
{
    [SerializeField] private CockpitViewCameraMovement cockpitCam;
    [SerializeField] private MountedCameraMovement mountedCam;

    // Reference to the camera's transform (insert in inspector)
    [SerializeField] private Transform cameraTransform;

    // Movement variables (editable in inspector)
    [SerializeField] private float forwardAcceleration = 3f;     // Acceleration in forward/backward direction
    [SerializeField] private float maxForwardSpeed = 8f;        // Max speed for forward/backward movement
    [SerializeField] private float maxStrafeSpeed = 7f;          // Max speed for strafing left/right
    [SerializeField] private float maxVerticalSpeed = 5f;        // Max speed for vertical movement (up/down)
    [SerializeField] private float decelerationRate = 2f;        // How quickly the submarine decelerates when no input is given

    private Rigidbody rb; // Reference to the submarine's Rigidbody component
    private InterpolateRotation followCamRotation;

    private bool inCockpit;
    private bool usingSubmarineAxes; // Flag to determine if the movement is based on the submarine's axes

    private void Start()
    {
        inCockpit = true;
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        followCamRotation = GetComponent<InterpolateRotation>();
        rb.useGravity = false;          // Disable gravity for the submarine, as it's in water
        rb.drag = 2f;                   // Apply some drag for a heavy underwater feel

        // Freeze the rotation of the submarine's Rigidbody to prevent rotation from collisions
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        // Handle camera switching
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchCamera();
        }

        // Check if the right mouse button is held down for free look
        if (Input.GetMouseButton(1))
        {
            usingSubmarineAxes = true;  // Use submarine's axes for movement
            followCamRotation.enabled = false;
        }
        else
        {
            usingSubmarineAxes = false; // Revert to using the camera's axes for movement
            followCamRotation.enabled = true;
        }

        if (inCockpit)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = Vector3.zero;

        // Use submarine's axes or camera's axes based on the state of right-click
        Transform referenceTransform = usingSubmarineAxes ? transform : cameraTransform;

        // Forward/Backward movement
        if (Input.GetKey(KeyCode.W))
        {
            inputDirection += referenceTransform.forward;  // Move forward according to the chosen transform's forward (local space)
        }
        else if (Input.GetKey(KeyCode.S))
        {
            inputDirection -= referenceTransform.forward;  // Move backward according to the chosen transform's forward (local space)
        }

        // Left/Right strafing
        if (Input.GetKey(KeyCode.A))
        {
            inputDirection -= referenceTransform.right;   // Strafe left according to the chosen transform's right (local space)
        }
        else if (Input.GetKey(KeyCode.D))
        {
            inputDirection += referenceTransform.right;   // Strafe right according to the chosen transform's right (local space)
        }

        // Ascend/Descend movement using world space Y-axis
        if (Input.GetKey(KeyCode.Space))
        {
            inputDirection += Vector3.up;  // Ascend (move up) using world space up (Vector3.up)
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            inputDirection -= Vector3.up;  // Descend (move down) using world space down (-Vector3.up)
        }

        ApplyMovement(inputDirection.normalized); // Normalize direction to ensure balanced movement
    }

    private void ApplyMovement(Vector3 inputDirection)
    {
        // Current velocity in world space
        Vector3 worldVelocity = rb.velocity;

        // Apply movement in the direction of the chosen axes
        if (inputDirection != Vector3.zero)
        {
            worldVelocity += inputDirection * forwardAcceleration * Time.deltaTime;
        }
        else
        {
            // Smoothly decelerate when no input is given
            worldVelocity = Vector3.Lerp(worldVelocity, Vector3.zero, decelerationRate * Time.deltaTime);
        }

        // Clamp the velocity to max speed limits
        worldVelocity.x = Mathf.Clamp(worldVelocity.x, -maxStrafeSpeed, maxStrafeSpeed);
        worldVelocity.y = Mathf.Clamp(worldVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);
        worldVelocity.z = Mathf.Clamp(worldVelocity.z, -maxForwardSpeed, maxForwardSpeed);

        // Set the Rigidbody's velocity to the calculated world velocity
        rb.velocity = worldVelocity;
    }

    private void SwitchCamera()
    {
        if (cockpitCam.gameObject.activeInHierarchy)
        {
            cockpitCam.Deactivate();
            mountedCam.Activate();
            inCockpit = false;
        }
        else
        {
            mountedCam.Deactivate();
            cockpitCam.Activate();
            inCockpit = true;
        }
    }
}