using System.Collections.Generic;
using UnityEngine;
using static CreatureMovementBehaviour;

[CreateAssetMenu(fileName = "New Sea Krait Movement", menuName = "Creature Movement/Sea Krait")]
public class SeaKraitMovement : CreatureMovementBehaviour
{
    private Creature myCreature;

    [Header("Sea Krait Specific Data")]
    [SerializeField] private float maxHeadRotationAngle = 10;
    [SerializeField] private float headRotationSpeed = 2;
    private Transform headTransform;

    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float steerSpeed = 0.5f;

    [SerializeField] private float obstacleAvoidanceWeight = 3;
    private List<Vector3> rayDirections;
    private float recalculationTimer = 0f;
    private Vector3 currentAvoidance;

    private Area bounds;
    [SerializeField] private float targetPointWeight = 0.75f;
    private Vector3 targetPoint;
    private float targetPointReachedDistance = 1;
    private Channel curChannel;
    private bool inChannel;

    #region Recalculation
    private float targetRecalculationTimer = 0;
    private float targetRecalculationTime = 1f;
    private float oldDistance;
    // If distance didnt change at least 'recalculationMargin' units, we recalculate
    [Header("Target Points")]
    [Tooltip("If the distance to the current target point doesn't change by this amount for one second, it will recalculate")]
    [SerializeField] private float recalculationMargin = 0.5f;
    #endregion

    [Header("Obstacle Avoidance Tweaking")]
    [SerializeField] private int rayCount = 33;
    [SerializeField] private float raycastLength = 5;
    [SerializeField] private float recalculationInterval = 0.33f;

    private float appliedMoveSpeed;

    private Rigidbody rb;
    public override void InitializeMovement(Creature origin)
    {
        // Setting MovementState to standard, every creature will initialize with standard movement
        curState = MovementState.Standard;

        myCreature = origin;
        bounds = origin.GetBounds();

        rayDirections = new();

        InitializeRayDirections();

        recalculationTimer = recalculationInterval;

        appliedMoveSpeed = moveSpeed;

        rb = myCreature.GetComponent<Rigidbody>();

        targetPoint = bounds.GetRandomPointWithin();

        headTransform = myCreature.GetHeadTransform();
    }
    public override void UpdateMovement()
    {
        switch (curState)
        {
            case MovementState.Standard:

                #region Movement

                rb.linearVelocity = Vector3.zero;

                UpdateTargetPoint();

                if (recalculationTimer >= recalculationInterval || currentAvoidance == Vector3.zero)
                {
                    currentAvoidance = FindUnobstructedDirection();
                    recalculationTimer = 0f;
                }

                recalculationTimer += Time.deltaTime;

                Vector3 unobstructedDirection = currentAvoidance.normalized * obstacleAvoidanceWeight;

                Vector3 targetPointForce = (targetPoint - myCreature.transform.position).normalized * targetPointWeight;

                Vector3 finalForce = unobstructedDirection + targetPointForce;

                Move(finalForce);

                #endregion

                #region Head Wobble

                headTransform.localRotation = Quaternion.Euler(maxHeadRotationAngle * Mathf.Cos(headRotationSpeed * Time.time), maxHeadRotationAngle * Mathf.Sin(headRotationSpeed * Time.time), 0);

                #endregion

                break;
        }
    }

    public void UpdateTargetPoint()
    {
        targetRecalculationTimer += Time.deltaTime;
        if (targetRecalculationTimer >= targetRecalculationTime)
        {
            float newDistance = (myCreature.transform.position - targetPoint).magnitude;

            if (Mathf.Abs(newDistance - oldDistance) < recalculationMargin)
            {
                targetPoint = bounds.GetRandomPointWithin();
            }

            oldDistance = (myCreature.transform.position - targetPoint).magnitude;
            targetRecalculationTimer = 0;
        }

        if ((myCreature.transform.position - targetPoint).magnitude < targetPointReachedDistance)
        {
            if (!inChannel)
            {
                float i = Random.Range((float)0, 1);
                if (i < 0.75f || !bounds.HasChannels()) targetPoint = bounds.GetRandomPointWithin();
                else
                {
                    // Debug.Log("Entering Channel");
                    curChannel = bounds.GetClosestChannel(myCreature.transform.position);
                    inChannel = true;
                    targetPoint = curChannel.GetEntrance(myCreature.transform.position, bounds);
                    if (curChannel.IsCompleted())
                    {
                        // Debug.Log("Channel ended early");
                        inChannel = false;
                        targetPoint = bounds.GetRandomPointWithin();
                    }

                }
            }
            if (inChannel)
            {
                if (curChannel.IsCompleted())
                {
                    // Debug.Log("Completed Channel");
                    inChannel = false;
                    targetPoint = bounds.GetRandomPointWithin();
                }
                else
                {
                    // Debug.Log("Traversing Channel");
                    targetPoint = curChannel.GetNext();
                }
            }
        }
    }
    private Vector3 FindUnobstructedDirection()
    {
        float sphereRadius = 0.15f;

        if (Physics.SphereCast(myCreature.transform.position, sphereRadius, myCreature.transform.forward, out RaycastHit useless, raycastLength, ~Constants.LAYER_CREATURE))
        {
            Vector3 preferredDirection = myCreature.transform.forward;
            float furthestUnobstructedDistance = 0;

            for (int i = 0; i < rayCount; i++)
            {
                Vector3 dir = myCreature.transform.TransformDirection(rayDirections[i]);

                // Throws a sphere with the boid's width as its radius into the given direction, ignores other boids in the collision detection
                if (Physics.SphereCast(myCreature.transform.position, sphereRadius, dir, out RaycastHit hit, raycastLength, ~Constants.LAYER_BOID))
                {
                    if (hit.distance > furthestUnobstructedDistance)
                    {
                        // prefers obstacles farther away from the boid over those closer to it
                        preferredDirection = dir;
                        furthestUnobstructedDistance = hit.distance;
                    }
                }
                else
                {
                    dir = dir.normalized;

                    // Interrupts the loop to return the clear path if the SphereCast doesn't return a hit

                    // DisplayDebugSphere(dir, Color.green, 0.15f);

                    return dir;
                }
            }

            // When all rays hit an obstacle, return the direction where the hit obstacle is farthest away
            preferredDirection = preferredDirection.normalized;

            // DisplayDebugSphere(preferredDirection, Color.green, 0.1f) ;

            return preferredDirection;
        }
        return Vector3.zero;
    }
    private void InitializeRayDirections()
    {
        rayDirections.Clear();

        float phi = (1 + Mathf.Sqrt(5)) / 2; // the golden ration, we love the golden ratio, big hand for the golden ratio

        for (int i = 0; i < rayCount; i++)
        {
            float t = (float)i / (rayCount - 1);  // returns a value from 0 to 1 throughout the loop
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = 2 * Mathf.PI * i / phi; // Azimuth is the angular distance from the north or south point of a sphere to any point on the sphere

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            rayDirections.Add(new Vector3(x, y, z).normalized); // Store direction vector in the list, so we don't do this bollocks every frame

            // DisplayDebugSphere(new Vector3(x, y, z) * raycastLength, Color.red, 0.075f);
        }
    }
    private void Move(Vector3 finalForce)
    {
        if (finalForce != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalForce);

            myCreature.transform.rotation = Quaternion.Slerp(myCreature.transform.rotation, targetRotation, Time.deltaTime * steerSpeed);
        }

        myCreature.transform.position += appliedMoveSpeed * Time.deltaTime * myCreature.transform.forward;
    }
}
