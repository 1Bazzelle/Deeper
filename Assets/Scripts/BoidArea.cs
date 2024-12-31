using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BoidArea : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private float forceActivationDistance;

    public float GetRadius()
    {
        return radius;
    }
    public float GetMinDistance()
    {
        return forceActivationDistance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(1, 0, 0) * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-1, 0, 0) * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 1, 0) * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -1, 0) * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 0, 1) * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 0, -1) * radius);

        Gizmos.DrawLine(transform.position, transform.position + new Vector3(1, 1, 1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(1, 1, -1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(1, -1, -1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-1, -1, -1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-1, -1, 1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-1, 1, 1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(1, -1, 1).normalized * radius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-1, 1, -1).normalized * radius);

    }
}
