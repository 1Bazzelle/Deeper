using UnityEngine;

public class SubmarineMovement
{
    [System.Serializable]
    public struct MovementData
    {
        public float forwardAcceleration;     // Acceleration in forward/backward direction
        public float maxForwardSpeed;         // Max speed for forward/backward movement
        public float strafeAcceleration;      // Acceleration for strafing left/right
        public float maxStrafeSpeed;          // Max speed for strafing left/right
        public float verticalAcceleration;    // Acceleration for vertical movement (up/down)
        public float maxVerticalSpeed;        // Max speed for vertical movement (up/down)
        public float decelerationRate;        // Deceleration rate for all directions
    }

    private struct Inputs
    {
        public int forward;
        public int right;
        public int vertical;
    }

    private MovementData data;

    private Rigidbody rb;

    private Transform cockpitCam;

    private float forwardSpeed = 0;
    private float strafeSpeed = 0;
    private float verticalSpeed = 0;

    public void Initialize(MovementData data, Rigidbody rb, Transform cockpitCam)
    {
        this.data = data;
        this.rb = rb;
        this.cockpitCam = cockpitCam;
    }
    public void UpdateMovement()
    {
        Inputs inputDirection;

        inputDirection.forward = 0;
        inputDirection.right = 0;
        inputDirection.vertical = 0;

        Transform referenceTransform = cockpitCam.transform;

        // Forward/Backward movement
        if (Input.GetKey(KeyCode.W))
        {
            forwardSpeed += data.forwardAcceleration * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            forwardSpeed -= data.forwardAcceleration * Time.deltaTime;
        }
        else
        {
            forwardSpeed = Mathf.Lerp(forwardSpeed, 0, data.decelerationRate * Time.deltaTime);
        }
        forwardSpeed = Mathf.Clamp(forwardSpeed, -data.maxForwardSpeed, data.maxForwardSpeed);

        // Left/Right strafing
        if (Input.GetKey(KeyCode.D))
        {
            strafeSpeed += data.strafeAcceleration * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            strafeSpeed -= data.strafeAcceleration * Time.deltaTime;
        }
        else
        {
            strafeSpeed = Mathf.Lerp(strafeSpeed, 0, data.decelerationRate * Time.deltaTime);
        }
        strafeSpeed = Mathf.Clamp(strafeSpeed, -data.maxStrafeSpeed, data.maxStrafeSpeed);

        // Ascend/Descend movement using world space Y-axis
        if (Input.GetKey(KeyCode.Space))
        {
            verticalSpeed += data.verticalAcceleration * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
        {
            verticalSpeed -= data.verticalAcceleration * Time.deltaTime;
        }
        else
        {
            verticalSpeed = Mathf.Lerp(verticalSpeed, 0, data.decelerationRate * Time.deltaTime);
        }
        verticalSpeed = Mathf.Clamp(verticalSpeed, -data.maxVerticalSpeed, data.maxVerticalSpeed);

        ApplyMovement();
    }
    private void ApplyMovement()
    {
        // Set the velocity along the submarine's forward direction
        Vector3 newVelocity = (forwardSpeed * cockpitCam.transform.forward + strafeSpeed * cockpitCam.transform.right + verticalSpeed * Vector3.up) / 3;

        // Apply the new velocity to the Rigidbody
        rb.linearVelocity = newVelocity;
    }

    public void Decelerate()
    {
        forwardSpeed = Mathf.Lerp(forwardSpeed, 0, data.decelerationRate * Time.deltaTime);
        strafeSpeed = Mathf.Lerp(strafeSpeed, 0, data.decelerationRate * Time.deltaTime);
        verticalSpeed = Mathf.Lerp(verticalSpeed, 0, data.decelerationRate * Time.deltaTime);

        ApplyMovement();
    }
}