using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine2 : MonoBehaviour
{
    [SerializeField] private MountedCameraMovement2 mountedCam;

    [Header("Submarine Movement Settings")]
    [SerializeField] private float forwardAcceleration = 10f; // Acceleration rate for forward movement
    [SerializeField] private float maxForwardSpeed = 20f; // Maximum forward speed
    [SerializeField] private float verticalAcceleration = 5f; // Acceleration rate for vertical movement
    [SerializeField] private float maxVerticalSpeed = 10f; // Maximum vertical speed
    [SerializeField] private float rotationAcceleration = 5f; // Acceleration rate for rotation
    [SerializeField] private float maxRotationSpeed = 50f; // Maximum rotation speed

    private float currentForwardSpeed = 0f;
    private float currentVerticalSpeed = 0f;
    private float currentRotationSpeed = 0f;

    [Header("Cockpit Camera Settings")]
    [SerializeField] private Transform cockpitCamera; // Assign the camera here
    [SerializeField] private float lookSpeed = 2f; // Speed of looking around
    [SerializeField] private Vector2 horizontalLookLimit = new Vector2(-70f, 70f); // Left/right limit
    [SerializeField] private Vector2 verticalLookLimit = new Vector2(-30f, 30f); // Up/down limit

    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;

    private bool inCockpit;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inCockpit = true;
    }

    void Update()
    {
        if (inCockpit)
        {
            HandleForwardMovement();
            HandleVerticalMovement();
            HandleRotation();

            HandleCameraLook();
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            SwitchCamera();
        }
    }
    private void SwitchCamera()
    {
        if (inCockpit)
        {
            mountedCam.Activate();
            inCockpit = false;
            cockpitCamera.gameObject.SetActive(false);
        }
        else
        {
            mountedCam.Deactivate();
            inCockpit = true;
            cockpitCamera.gameObject.SetActive(true);
        }
    }

    private void HandleForwardMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            currentForwardSpeed += forwardAcceleration * Time.deltaTime;
            currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, 0, maxForwardSpeed);
        }
        else
        {
            // Gradually slow down when W is not pressed
            currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, 0, Time.deltaTime * 2f);
        }
    }

    private void HandleVerticalMovement()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            currentVerticalSpeed += verticalAcceleration * Time.deltaTime;
            currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, 0, maxVerticalSpeed);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentVerticalSpeed -= verticalAcceleration * Time.deltaTime;
            currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, -maxVerticalSpeed, 0);
        }
        else
        {
            // Slow down vertical speed when neither key is pressed
            currentVerticalSpeed = Mathf.Lerp(currentVerticalSpeed, 0, Time.deltaTime * 2f);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.A))
        {
            currentRotationSpeed -= rotationAcceleration * Time.deltaTime;
            currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed, maxRotationSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            currentRotationSpeed += rotationAcceleration * Time.deltaTime;
            currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed, maxRotationSpeed);
        }
        else
        {
            // Gradually slow down rotation when neither A nor D is pressed
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, 0, Time.deltaTime * 2f);
        }

        transform.Rotate(0, currentRotationSpeed * Time.deltaTime, 0);
    }

    private void HandleCameraLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        // Update camera rotation within limits
        cameraRotationX = Mathf.Clamp(cameraRotationX + mouseX, horizontalLookLimit.x, horizontalLookLimit.y);
        cameraRotationY = Mathf.Clamp(cameraRotationY - mouseY, verticalLookLimit.x, verticalLookLimit.y); // Invert Y for natural feel

        cockpitCamera.localRotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0f);
    }

    private void FixedUpdate()
    {
        // Apply forward and vertical movement
        rb.velocity = transform.forward * currentForwardSpeed + transform.up * currentVerticalSpeed;

        // Apply rotation around Y axis
        rb.angularVelocity = transform.up * currentRotationSpeed * Mathf.Deg2Rad;
    }
}
