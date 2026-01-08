using UnityEngine;

public class CockpitCamera
{
    [System.Serializable]
    public struct CockpitCameraData
    {
        [Header("Acceleration & Deceleration")]
        public float horizontalAcceleration;
        public float verticalAcceleration;
        public float maxHorizontalSpeed;
        public float maxVerticalSpeed;
        public float decelerationFactor;

        [Header("Bounds")]
        public float maxUpRotation;
        public float maxDownRotation;
    }

    private CockpitCameraData camData;
    private Camera cam;

    private float horizontalSpeed = 0.0f;
    private float verticalSpeed = 0.0f;

    private Vector3 initialRotation;

    private Transform followTransform;


    public void Initialize(CockpitCameraData camData, Camera cam, Transform followTransform)
    {
        this.camData = camData;

        this.cam = cam;
        initialRotation = cam.transform.localEulerAngles;

        this.followTransform = followTransform;
    }

    public void UpdateCamera()
    {
        cam.transform.position = new Vector3(followTransform.position.x, followTransform.position.y, followTransform.position.z);

        if (UIManager.Instance.GetCurScreen() != UIManager.ScreenID.Journal) LockCursor();

        // Get current mouse movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Check if mouse is moving horizontally
        if (Mathf.Abs(mouseX) > 0.01f) // Small threshold to avoid jittering
        {
            // Accelerate the horizontal speed based on mouse movement
            horizontalSpeed += mouseX * camData.horizontalAcceleration * Constants.mouseSensitivity;
        }
        else
        {
            // Decelerate the horizontal speed if the mouse stops moving
            horizontalSpeed = Mathf.Lerp(horizontalSpeed, 0, camData.decelerationFactor * Time.deltaTime);
        }

        // Check if mouse is moving vertically
        if (Mathf.Abs(mouseY) > 0.01f)
        {
            // Accelerate the vertical speed based on mouse movement (inverted Y)
            verticalSpeed += -mouseY * camData.verticalAcceleration * Constants.mouseSensitivity;
        }
        else
        {
            // Decelerate the vertical speed if the mouse stops moving
            verticalSpeed = Mathf.Lerp(verticalSpeed, 0, camData.decelerationFactor * Time.deltaTime);
        }

        // Clamp the speeds to the max allowed values
        horizontalSpeed = Mathf.Clamp(horizontalSpeed, -camData.maxHorizontalSpeed, camData.maxHorizontalSpeed);
        verticalSpeed = Mathf.Clamp(verticalSpeed, -camData.maxVerticalSpeed, camData.maxVerticalSpeed);

        // Calculate the desired new rotation angles
        Vector3 newRotation = cam.transform.localEulerAngles + new Vector3(verticalSpeed * Time.deltaTime, horizontalSpeed * Time.deltaTime, 0.0f);

        // Calculate relative rotation compared to the initial rotation
        float relativeVerticalRotation = Mathf.DeltaAngle(initialRotation.x, newRotation.x);

        // Vertical boundary behavior: decelerate smoothly near boundaries
        if (relativeVerticalRotation > camData.maxUpRotation)
        {
            // If we're trying to move further upwards, smoothly decelerate
            if (verticalSpeed > 0)
            {
                verticalSpeed = Mathf.Lerp(verticalSpeed, 0, camData.decelerationFactor * Time.deltaTime);
            }
            newRotation.x = initialRotation.x + camData.maxUpRotation;
        }
        else if (relativeVerticalRotation < -camData.maxDownRotation)
        {
            // If we're trying to move further downwards, smoothly decelerate
            if (verticalSpeed < 0)
            {
                verticalSpeed = Mathf.Lerp(verticalSpeed, 0, camData.decelerationFactor * Time.deltaTime);
            }
            newRotation.x = initialRotation.x - camData.maxDownRotation;
        }

        // Apply the updated rotation without clamping the horizontal rotation
        cam.transform.localEulerAngles = newRotation;
    }

    private void LockCursor()
    {
        // Lock the cursor and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
