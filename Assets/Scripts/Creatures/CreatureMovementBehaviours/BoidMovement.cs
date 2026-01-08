using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boid Movement", menuName = "Creature Movement/Boid Movement")]
public class BoidMovement : CreatureMovementBehaviour
{
    private Creature myCreature;

    [Tooltip("Sight range only for vision range to see other boids")]
    [SerializeField] private float sightRange = 5;
    [SerializeField] private float seperationRange = 0.5f;

    [SerializeField] private float visionAngle = 135;

    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float steerSpeed = 2.5f;

    [Header("Behaviour Weighting")]
    [SerializeField] private float seperationWeight = 1;
    [SerializeField] private float alignmentWeight = 1;
    [SerializeField] private float cohesionWeight = 1;
    private List<GameObject> nearbyBoids;
    private BoidMovement leaderBoid;

    [SerializeField] private float generalBoidBehaviourWeight = 1;

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

        nearbyBoids = new();
        rayDirections = new();

        InitializeRayDirections();

        recalculationTimer = recalculationInterval;

        appliedMoveSpeed = moveSpeed;

        rb = myCreature.GetComponent<Rigidbody>();

        targetPoint = bounds.GetRandomPointWithin();
    }
    public override void UpdateMovement()
    {
        if (myCreature.showGizmos) Gizmos();

        switch (curState)
        {
            case MovementState.Standard:

                #region boid boid boid

                rb.linearVelocity = Vector3.zero;

                UpdateNearbyBoids();

                UpdateTargetPoint();

                if (recalculationTimer >= recalculationInterval || currentAvoidance == Vector3.zero)
                {
                    currentAvoidance = FindUnobstructedDirection();
                    recalculationTimer = 0f;
                }

                recalculationTimer += Time.deltaTime;

                (Vector3 Seperation, Vector3 Alignment, Vector3 Cohesion) boidBehaviour = GetBoidVectors();

                Vector3 boidForce = (
                                       (boidBehaviour.Seperation * seperationWeight)
                                     + (boidBehaviour.Alignment * alignmentWeight)
                                     + (boidBehaviour.Cohesion * cohesionWeight)
                                     ).normalized * generalBoidBehaviourWeight;

                Vector3 unobstructedDirection = currentAvoidance.normalized * obstacleAvoidanceWeight;

                Vector3 targetPointForce = (targetPoint - myCreature.transform.position).normalized * targetPointWeight;

                Vector3 finalForce = boidForce + unobstructedDirection + targetPointForce;

                Move(finalForce);

                #endregion

                break;
        }
    }

    private void UpdateNearbyBoids()
    {
        nearbyBoids.Clear();

        Collider[] creaturesArray = Physics.OverlapSphere(myCreature.transform.position, sightRange, Constants.LAYER_CREATURE);
        List<Collider> boids = new();

        for (int i = 0; i < creaturesArray.Length; i++)
        {
            Creature curCreature = creaturesArray[i].GetComponent<Creature>();

            if (curCreature.IsBoid() && curCreature.id == myCreature.id)
            {
                boids.Add(creaturesArray[i]);
            }
        }

        if (boids.Count == 0 && leaderBoid != this)
        {
            // Debug.Log("No other boid found");
            BecomeLeader();
        }

        bool foundLeader = false;

        for (int i = 0; i < boids.Count; i++)
        {
            Creature curCreature = boids[i].GetComponent<Creature>();

            Vector3 fromTo = curCreature.transform.position - myCreature.transform.position;

            // Is the boid iterating over itself?
            if (curCreature.gameObject == myCreature.gameObject) continue;

            BoidMovement boid = curCreature.GetBoidMovement();
            // Is it?
            if (boid == this) continue;

            #region Leader Check

            // If I am Leader and see boid that thinks they is Leader
            if (leaderBoid == this && boid.GetLeader() == boid)
            {
                // Debug.Log("Ay bozo, what do you think youre doing");

                // Whoever's Forward Vector has a negative value in the dot product to the fromTo vector, remains leader
                Vector3 meToThey = boid.GetCreature().transform.position - myCreature.transform.position;

                if (Vector3.Dot(myCreature.transform.forward, meToThey) < 0)
                {
                    // Debug.Log("Take my privilege");
                    foundLeader = true;
                    leaderBoid = boid;
                }
            }

            if (leaderBoid == boid)
            {
                foundLeader = true;
                break;
            }
            #endregion

            // Check if boid is in vision Range
            if (Vector3.Angle(myCreature.transform.forward, fromTo) < visionAngle)
            {
                nearbyBoids.Add(curCreature.gameObject);
            }
        }
        if (leaderBoid != this && foundLeader == false)
        {
            // Debug.Log("I am become Leader");
            BecomeLeader();
        }

        // Only for marking Leaders
        if (leaderBoid == this)
        {
            // GameManager.Instance.DisplayDebugSphere(myCreature.transform, Color.yellow, 0.5f);
        }
    }
    public void UpdateTargetPoint()
    {
        // If I am not the leader, I just follow the big man
        if (leaderBoid != this)
        {
            targetPoint = leaderBoid.GetTargetPoint();

            if (leaderBoid.GetInChannel())
            {
                inChannel = true;
                curChannel = leaderBoid.GetChannel();
            }
            else
            {
                inChannel = false;
            }
        }

        if (leaderBoid == this && (myCreature.transform.position - targetPoint).magnitude < targetPointReachedDistance)
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
        if (leaderBoid == this)
        {
            // GameManager.Instance.DisplayDebugSphereWorldSpace(targetPoint, Color.green, 0.2f);
        }
    }

    private (Vector3 Seperation, Vector3 Alignment, Vector3 Cohesion) GetBoidVectors()
    {
        Vector3 seperationVector = Vector3.zero;
        Vector3 alignmentVector = Vector3.zero;
        Vector3 cohesionVector = Vector3.zero;

        // This is where the boid happens
        for (int i = 0; i < nearbyBoids.Count; i++)
        {
            #region Seperation
            Vector3 diff = myCreature.transform.position - nearbyBoids[i].transform.position;
            float dist = diff.magnitude;

            if (dist < seperationRange)
            {
                seperationVector += diff.normalized;
            }
            #endregion

            #region Alignment
            alignmentVector += nearbyBoids[i].transform.forward;
            #endregion

            #region Cohesion
            cohesionVector += nearbyBoids[i].transform.position;
            #endregion
        }

        cohesionVector /= nearbyBoids.Count;
        cohesionVector = cohesionVector - myCreature.transform.position;

        seperationVector = seperationVector.normalized;
        alignmentVector = alignmentVector.normalized;
        cohesionVector = cohesionVector.normalized;

        return (seperationVector, alignmentVector, cohesionVector);
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

    private void BecomeLeader()
    {
        // Debug.Log("Becomes Leader");
        leaderBoid = this;
        targetPoint = bounds.GetRandomPointWithin();
    }

    #region Getters/Setters
    public Creature GetCreature()
    {
        return myCreature;
    }
    public BoidMovement GetLeader()
    {
        return leaderBoid;
    }
    public Vector3 GetTargetPoint()
    {
        return targetPoint;
    }
    public bool GetInChannel()
    {
        return inChannel;
    }
    public Channel GetChannel()
    {
        return curChannel;
    }
    #endregion

    #region Gizmos
    private void Gizmos()
    {
        if (leaderBoid == this)
        {
            GameManager.Instance.DisplayDebugSphere(myCreature.transform, Color.yellow, 0.25f);
            GameManager.Instance.DisplayDebugSphereWorldSpace(targetPoint, Color.blue, 0.25f);
        }
    }
    #endregion

}
