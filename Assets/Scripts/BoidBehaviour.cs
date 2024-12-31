using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidBehaviour : MonoBehaviour
{
    [Header("Boid Data")]
    [SerializeField] private BoidArea boidArea;

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

    [SerializeField] private float obstacleAvoidanceWeight;

    [SerializeField] private float boidAreaForceWeight;

    [Header("Obstacle Avoidance Tweaking")]
    [SerializeField] private int rayCount = 30;
    [SerializeField] private float raycastLength;
    [SerializeField] private float recalculationInterval = 0.33f;

    private List<GameObject> nearbyBoids;

    private List<Vector3> rayDirections;

    private float recalculationTimer = 0f;
    private Vector3 currentAvoidance;

    private float appliedMoveSpeed;

    void Start()
    {
        nearbyBoids = new();
        rayDirections = new();

        InitializeRayDirections();

        recalculationTimer = recalculationInterval;

        appliedMoveSpeed = moveSpeed;
    }

    void Update()
    {
        UpdateNearbyBoids();

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
                             ).normalized;

        Vector3 unobstructedDirection = currentAvoidance * obstacleAvoidanceWeight;

        Vector3 vectorToAreaCenter = boidArea.transform.position - transform.position;
        Vector3 forceTowardAreaCenter = Vector3.zero;

        if(vectorToAreaCenter.magnitude > boidArea.GetMinDistance())
        {
            forceTowardAreaCenter = vectorToAreaCenter.normalized * (vectorToAreaCenter.magnitude / boidArea.GetRadius());
            forceTowardAreaCenter *= boidAreaForceWeight;

            float mag = forceTowardAreaCenter.magnitude;

            forceTowardAreaCenter = forceTowardAreaCenter.normalized + boidForce + unobstructedDirection;
            forceTowardAreaCenter = forceTowardAreaCenter.normalized * mag;
        }

        Vector3 finalForce = boidForce + unobstructedDirection + forceTowardAreaCenter;

        Move(finalForce);
    }

    private void UpdateNearbyBoids()
    {
        nearbyBoids.Clear();

        foreach (Collider c in Physics.OverlapSphere(transform.position, sightRange, Constants.LAYER_BOID))
        {
            Vector3 fromTo = c.transform.position - transform.position;

            if(Vector3.Angle(transform.forward, fromTo) < visionAngle)
                nearbyBoids.Add(c.gameObject);
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
                seperationVector += diff.normalized / dist;
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

            // When all rays hit an obstacle, return the direction where the obstacle is furthest away
            preferredDirection = preferredDirection.normalized;

            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2.GetComponent<Renderer>().material.color = Color.green;
            sphere2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            sphere2.transform.position = transform.position + preferredDirection;
            sphere2.transform.SetParent(transform, true);
            Destroy(sphere2.GetComponent<SphereCollider>());

            Destroy(sphere2, 0.1f);

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
            
            /*
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
            sphere.transform.position = transform.position + new Vector3(x, y, z) * raycastLength;
            sphere.transform.SetParent(transform, true);
            Destroy(sphere.GetComponent<SphereCollider>());
            // */
        }
    }

    private void Move(Vector3 finalForce)
    {
        if (finalForce != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalForce);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * steerSpeed);
        }

        transform.position += transform.forward * appliedMoveSpeed * Time.deltaTime;
    }

    private void DisplayDebugSphere(Vector3 direction, Color color, float scale)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.position = transform.position + direction;
        sphere.transform.SetParent(transform, true);
        Destroy(sphere.GetComponent<SphereCollider>());
        Destroy(sphere, 0.1f);
    }
}
