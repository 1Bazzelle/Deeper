using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidBehaviour : MonoBehaviour
{

    [SerializeField] private bool showGizmos;
    [Header("Boid Data")]
    [SerializeField] private int boidID;

    [Tooltip("Sight range only for vision range to see other boids")]
    [SerializeField] private float sightRange;
    [SerializeField] private float seperationRange;

    [SerializeField] private float visionAngle;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float steerSpeed;

    [SerializeField] private float catchUpDistance;
    [SerializeField] private float catchUpBonus;

    [Header("Behaviour Weighting")]
    [SerializeField] private float seperationWeight;
    [SerializeField] private float alignmentWeight;
    [SerializeField] private float cohesionWeight;
    private List<GameObject> nearbyBoids;
    private BoidBehaviour leaderBoid;

    [SerializeField] private float generalBoidBehaviourWeight;

    [SerializeField] private float obstacleAvoidanceWeight;
    private List<Vector3> rayDirections;
    private float recalculationTimer = 0f;
    private Vector3 currentAvoidance;

    [SerializeField] private Area bounds;
    [SerializeField] private float targetPointWeight;
    private Vector3 targetPoint;
    private float targetPointReachedDistance = 1;
    private Channel curChannel;
    private bool inChannel;

    [Header("Leader Boid Calculations")]

    [Header("Obstacle Avoidance Tweaking")]
    [SerializeField] private int rayCount = 30;
    [SerializeField] private float raycastLength;
    [SerializeField] private float recalculationInterval = 0.33f;

    private float appliedMoveSpeed;

    private Rigidbody rb;
    void Start()
    {
        nearbyBoids = new();
        rayDirections = new();

        InitializeRayDirections();

        recalculationTimer = recalculationInterval;

        appliedMoveSpeed = moveSpeed;

        rb = GetComponent<Rigidbody>();

        targetPoint = bounds.GetRandomPointWithin();
    }

    void Update()
    {
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
                               ( boidBehaviour.Seperation * seperationWeight ) 
                             + ( boidBehaviour.Alignment  * alignmentWeight  ) 
                             + ( boidBehaviour.Cohesion   * cohesionWeight   )
                             ).normalized * generalBoidBehaviourWeight;

        Vector3 unobstructedDirection = currentAvoidance.normalized * obstacleAvoidanceWeight;

        Vector3 targetPointForce = (targetPoint - transform.position).normalized * targetPointWeight;

        Vector3 finalForce = boidForce + unobstructedDirection + targetPointForce;

        Move(finalForce);
    }

    public int GetBoidID()
    {
        return boidID;
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

    private void UpdateNearbyBoids()
    {
        nearbyBoids.Clear();

        Collider[] boidsArray = Physics.OverlapSphere(transform.position, sightRange, Constants.LAYER_BOID);
        List<Collider> boids = new();

        for (int i = 0; i < boidsArray.Length; i++)
        {
            BoidBehaviour boid = boidsArray[i].GetComponent<BoidBehaviour>();
            if (boid.GetBoidID() == boidID) boids.Add(boidsArray[i]);
        }
        if (boids.Count == 0)
        {
            // Debug.Log("No other boid found");
            BecomeLeader();
        }

        bool foundLeader = false;

        for (int i = 0; i < boids.Count; i++)
        {
            Collider c = boids[i];
            Vector3 fromTo = c.transform.position - transform.position;

            if (c.gameObject == gameObject) continue;

            BoidBehaviour boid = c.GetComponent<BoidBehaviour>();

            if (boid == this) continue;

            #region Leader Check
            // If I am Leader and see boid that thinks they is Leader

            if (leaderBoid == this && boid.GetLeader() == boid)
            {
                // Debug.Log("Ay bozo, what do you think youre doing");

                // Whoever's Forward Vector has a higher value in the dot product, remains leader
                Vector3 meToThey = boid.transform.position - transform.position;

                if (Vector3.Dot(transform.forward, meToThey) > 0)
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
            if (Vector3.Angle(transform.forward, fromTo) < visionAngle)
            {
                nearbyBoids.Add(c.gameObject);
            }
        }
        if (leaderBoid != this && foundLeader == false)
        {
            // Debug.Log("I am become Leader");
            BecomeLeader();
        }

        if (leaderBoid == this)
        {
            // DisplayDebugSphere(Vector3.zero, Color.yellow, 0.5f);
        }
    }
    private void UpdateTargetPoint()
    {
        // If I am not the leader, I just follow the big man
        if(leaderBoid != this)
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

        if (leaderBoid == this && (transform.position - targetPoint).magnitude < targetPointReachedDistance)
        {
            if (!inChannel)
            {
                float i = Random.Range((float)0, 1);
                if (i < 0.75f) targetPoint = bounds.GetRandomPointWithin();
                else
                {
                    // Debug.Log("Entering Channel");
                    curChannel = bounds.GetClosestChannel(transform.position);
                    inChannel = true;
                    targetPoint = curChannel.GetEntrance(transform.position, bounds);
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
            // DisplayDebugSphereWorldSpace(targetPoint, Color.green, 0.2f);
        }
    }
    private (Vector3 Seperation, Vector3 Alignment, Vector3 Cohesion) GetBoidVectors()
    {
        Vector3 seperationVector = Vector3.zero;
        Vector3 alignmentVector = Vector3.zero;
        Vector3 cohesionVector = Vector3.zero;

        for (int i = 0; i < nearbyBoids.Count; i++)
        {
            #region Seperation
            Vector3 diff = transform.position - nearbyBoids[i].transform.position;
            float dist = diff.magnitude;

            if (dist < seperationRange)
            {
                // Steer force will be higher for boids closer to this boid
                // seperationVector += diff.normalized / dist;
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
        cohesionVector = cohesionVector - transform.position;

        if (cohesionVector.magnitude > catchUpDistance) appliedMoveSpeed = moveSpeed * catchUpBonus;
        else appliedMoveSpeed = moveSpeed;

        seperationVector = seperationVector.normalized;
        alignmentVector  = alignmentVector.normalized;
        cohesionVector   = cohesionVector.normalized;

        return (seperationVector, alignmentVector, cohesionVector);
    }
    private Vector3 FindUnobstructedDirection()
    {
        float sphereRadius = 0.15f;
        
        if (Physics.SphereCast(transform.position, sphereRadius, transform.forward, out RaycastHit useless, raycastLength, ~Constants.LAYER_BOID))
        {
            Vector3 preferredDirection = transform.forward;
            float furthestUnobstructedDistance = 0;
            RaycastHit hit;

            for (int i = 0; i < rayCount; i++)
            {
                Vector3 dir = transform.TransformDirection(rayDirections[i]);
                
                // Throws a sphere with the boid's width as its radius into the given direction, ignores other boids in the collision detection
                if (Physics.SphereCast(transform.position, sphereRadius, dir, out hit, raycastLength, ~Constants.LAYER_BOID))
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

            //DisplayDebugSphere(preferredDirection, Color.green, 0.1f) ;

            return preferredDirection;
        }
        return Vector3.zero;
    }
    private void InitializeRayDirections()
    {
        rayDirections.Clear();

        float phi = (1 + Mathf.Sqrt(5)) / 2; // golden ration, we love the golden ratio, big hand for the golden ratio

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

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * steerSpeed);
        }

        transform.position += appliedMoveSpeed * Time.deltaTime * transform.forward;
    }

    private void BecomeLeader()
    {
        // Debug.Log("Becomes Leader");
        leaderBoid = this;
        targetPoint = bounds.GetRandomPointWithin();
    }
    public BoidBehaviour GetLeader()
    {
        return leaderBoid;
    }

    private void DisplayDebugSphere(Vector3 offset, Color color, float scale)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.position = transform.position + offset;
        sphere.transform.SetParent(transform, true);
        Destroy(sphere.GetComponent<SphereCollider>());
        Destroy(sphere, 0.025f);
    }
    private void DisplayDebugSphereWorldSpace(Vector3 worldPosition, Color color, float scale)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.position = worldPosition;
        Destroy(sphere.GetComponent<SphereCollider>());
        Destroy(sphere, 0.025f);
    }
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * raycastLength);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * sightRange);

        Gizmos.color = Color.red + Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * seperationRange);
    }
}
