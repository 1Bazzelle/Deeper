using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraTest : MonoBehaviour
{
    [SerializeField] private bool showGizmos;

    [SerializeField] private float moveSpeed;

    [SerializeField] private int rayAmountX;
    [SerializeField] private int rayAmountY;

    [SerializeField] private float range;

    [SerializeField] private float minValue;

    [Range(0.0f, 0.5f)]
    [SerializeField] private float fallOffStart;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float fallOffStrength;

    [SerializeField] private float minDistance;

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Camera cam = GetComponent<Camera>();
        if (cam == null) return;

        if (rayAmountX <= 1 || rayAmountY <= 1)
        {
            // Amounts must be greater than 1
            return;
        }

        float centralMaxX = 1.0f - fallOffStart;
        float centralMaxY = 1.0f - fallOffStart;

        for (int x = 0; x < rayAmountX; x++)
        {
            for (int y = 0; y < rayAmountY; y++)
            {
                float normalizedX = x / (float)(rayAmountX - 1);
                float normalizedY = y / (float)(rayAmountY - 1);

                Vector3 viewportPoint = new Vector3(normalizedX, normalizedY, cam.nearClipPlane);
                Vector3 worldPosition = cam.ViewportToWorldPoint(viewportPoint);

                Vector3 dir = (worldPosition - cam.transform.position).normalized;

                // Calculate the distance from the current point to the edges of the central rectangle
                float distanceToCenterX = Mathf.Max(0, Mathf.Abs(normalizedX - 0.5f) - (centralMaxX - 0.5f));
                float distanceToCenterY = Mathf.Max(0, Mathf.Abs(normalizedY - 0.5f) - (centralMaxY - 0.5f));

                // Calculate the combined falloff ratio based on the maximum distance
                float combinedFallOffRatio = Mathf.Max(distanceToCenterX, distanceToCenterY) / (0.5f - fallOffStart);
                combinedFallOffRatio = Mathf.Clamp01(combinedFallOffRatio);

                // Map fallOffStrength to an exponential scale for a wider range of falloff effects
                float scaledFallOffStrength = Mathf.Lerp(0.01f, 10.0f, fallOffStrength);
                float easedFallOff = Mathf.Pow(combinedFallOffRatio, scaledFallOffStrength);

                // Interpolate the ray value between maxValue (1) and minValue based on the falloff
                float rayValue = Mathf.Lerp(1, minValue, easedFallOff);

                // Interpolate color based on ray value
                Color rayColor = Color.Lerp(Color.green, Color.red, (1 - rayValue) / (1 - minValue));

                // Draw the ray with the computed color
                Gizmos.color = rayColor;
                //Gizmos.DrawLine(cam.transform.position, cam.transform.position + dir * range);

                Gizmos.DrawSphere(cam.transform.position + dir * range, 0.25f);
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(DetermineQuality(GetComponent<Camera>(), rayAmountX, rayAmountY, minDistance, range, minValue, fallOffStart, fallOffStrength));
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.x, transform.rotation.y+1, transform.rotation.z), moveSpeed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.x, transform.rotation.y-1, transform.rotation.z), moveSpeed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.x-1, transform.rotation.y, transform.rotation.z), moveSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.x+1, transform.rotation.y, transform.rotation.z), moveSpeed);
        }
    }

    private float DetermineQuality(Camera cam, int xRays, int yRays, float minRange, float range, float minValue, float fallOffStart, float fallOffStrength)
    {
        float quality = 0;

        // Later replace 'GameObject' with 'Creature'
        List<GameObject> hitObjects = new();

        float centralMaxX = 1.0f - fallOffStart;
        float centralMaxY = 1.0f - fallOffStart;

        for (int x = 0; x < xRays; x++)
        {
            for (int y = 0; y < yRays; y++)
            {
                float normalizedX = x / (float)(xRays - 1);
                float normalizedY = y / (float)(yRays - 1);

                Vector3 viewportPoint = new Vector3(normalizedX, normalizedY, cam.nearClipPlane);
                Vector3 worldPosition = cam.ViewportToWorldPoint(viewportPoint);

                Vector3 dir = (worldPosition - cam.transform.position).normalized;

                // Calculate the distance from the current point to the edges of the central rectangle
                float distanceToCenterX = Mathf.Max(0, Mathf.Abs(normalizedX - 0.5f) - (centralMaxX - 0.5f));
                float distanceToCenterY = Mathf.Max(0, Mathf.Abs(normalizedY - 0.5f) - (centralMaxY - 0.5f));

                // Calculate the combined falloff ratio based on the maximum distance
                float combinedFallOffRatio = Mathf.Max(distanceToCenterX, distanceToCenterY) / (0.5f - fallOffStart);
                combinedFallOffRatio = Mathf.Clamp01(combinedFallOffRatio);

                // Map fallOffStrength to an exponential scale for a wider range of falloff effects
                float scaledFallOffStrength = Mathf.Lerp(0.01f, 10.0f, fallOffStrength);
                float easedFallOff = Mathf.Pow(combinedFallOffRatio, scaledFallOffStrength);

                float rayValue = Mathf.Lerp(1, minValue, easedFallOff);

                if(Physics.Raycast(worldPosition, dir, out RaycastHit hit, range))
                {
                    if (!hitObjects.Contains(hit.collider.gameObject))
                    {
                        hitObjects.Add(hit.collider.gameObject);
                        Debug.Log("Hit object: " + hit.collider.gameObject.name);
                    }

                    quality += rayValue;
                }
            }
        }
        int count = hitObjects.Count;

        if (count == 0) return 0;

        return quality;
    }
}