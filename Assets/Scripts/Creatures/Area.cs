using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


[System.Serializable]
public struct Channel
{
    private Area origin;

    [SerializeField] private bool oneWay;
    [SerializeField] private List<Vector3> wayPoints;

    private int curIndex;
    private bool reverseOrder;

    private bool completed;

    public Channel(Vector3 initPos)
    {
        origin = null;
        curIndex = 0;
        completed = true;
        reverseOrder = false;
        wayPoints = new();
        oneWay = false;

        wayPoints.Add(initPos);
    }
    public Vector3 GetEntrance(Vector3 position, Area bounds)
    {
        origin = bounds;

        completed = false;
        if (wayPoints.Count == 0)
        {
            completed = true;
            return origin.GetCenter().position + position;
        }
        if (oneWay)
        {
            curIndex = 0;
            reverseOrder = false;
            return origin.GetCenter().position + wayPoints[0];
        }
        if ((wayPoints[0] - position).magnitude < (wayPoints[wayPoints.Count - 1] - position).magnitude)
        {
            curIndex = 0;
            reverseOrder = false;
            return origin.GetCenter().position + wayPoints[0];
        }
        else
        {
            reverseOrder = true;
            curIndex = wayPoints.Count - 1;
            return origin.GetCenter().position + wayPoints[wayPoints.Count - 1];
        }
    }
    public Vector3 GetNext()
    {
        // If curIndex out of bounds of way point list
        if (!reverseOrder && curIndex > wayPoints.Count - 2 || reverseOrder && curIndex < 0)
        {
            completed = true;
            return origin.GetRandomPointWithin();
        }
        // Traversing In-Order
        if (!reverseOrder)
        {
            return origin.GetCenter().position + wayPoints[curIndex++];
        }
        // Traversing in Reverse-Order
        else if (reverseOrder)
        {
            return origin.GetCenter().position + wayPoints[curIndex--];
        }
        return origin.GetRandomPointWithin();
    }
    public bool IsCompleted()
    {
        return completed;
    }
    public void DrawGizmos(Vector3 worldPos)
    {
        Gizmos.color = Color.red + Color.blue;

        for (int i = 0; i < wayPoints.Count - 1; i++)
        {
            Gizmos.DrawSphere(worldPos + wayPoints[i], 0.15f);
            Gizmos.DrawLine(worldPos + wayPoints[i], worldPos + wayPoints[i+1]);
        }

        if(wayPoints.Count > 0) Gizmos.DrawSphere(worldPos + wayPoints[wayPoints.Count - 1], 0.15f);
    }
}

[System.Serializable]
public struct AreaCenter
{
    public Vector3 position;
    public List<AreaDefiner> definers;

    public void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 0.5f);
    }
}

[System.Serializable]
public struct AreaDefiner
{
    public Vector3 position;
    public float radius;

    private float drawRange;

    public Vector3 GetRandomPointWithin()
    {
        Vector3 randomDir = Random.onUnitSphere;

        float randomDistance = Mathf.Pow(Random.Range(0f, 1f), 1f / 3f) * radius;

        return position + randomDir * randomDistance;
    }

    private List<Vector3> InitializeGizmos()
    {
        // All of this code is shameless stolen from the boid Obstacle Avoidance raycasts

        int amount = 50;

        List<Vector3> points = new();

        float phi = (1 + Mathf.Sqrt(5)) / 2; // the golden ration, we love the golden ratio, big hand for the golden ratio

        for (int i = 0; i < amount; i++)
        {
            float t = (float)i / (50 - 1);  // returns a value from 0 to 1 throughout the loop
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = 2 * Mathf.PI * i / phi; // Azimuth is the angular distance from the north or south point of a sphere to any point on the sphere

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            points.Add(position + new Vector3(x, y, z).normalized * radius);
        }

        return points;
    }
    public void DrawGizmos(Vector3 pos)
    {
        List<Vector3> points = InitializeGizmos();

        drawRange = radius * 0.75f;

        Gizmos.color = Color.blue;

        for (int i = 0; i < points.Count; i++)
        {
            for (int p = 0; p < points.Count; p++)
            {
                if (points[i] != points[p] && (points[i] - points[p]).magnitude < drawRange) Gizmos.DrawLine(pos + points[i], pos + points[p]);
            }
        }
    }
}

[CreateAssetMenu(fileName = "New Area", menuName = "Area")]
public class Area : ScriptableObject
{
    [SerializeField] private bool showGizmos;

    [SerializeField] private AreaCenter center;
    [SerializeField] private List<Channel> channels;

    public Vector3 GetRandomPointWithin()
    {
        return center.position + center.definers[Random.Range(0, center.definers.Count)].GetRandomPointWithin();
    }

    public Channel GetClosestChannel(Vector3 fishPos)
    {
        if (channels == null || channels.Count == 0) return new Channel(GetRandomPointWithin());

        Channel closest = channels[0];
        for (int i = 1; i< channels.Count - 1; i++)
        {
            if ((channels[i].GetEntrance(fishPos, this) - fishPos).magnitude < (closest.GetEntrance(fishPos, this) - fishPos).magnitude) closest = channels[i];
        }
        return closest;
    }

    public void DrawGizmos()
    {
        if (!showGizmos) return;

        center.DrawGizmos();

        for(int i = 0; i < center.definers.Count; i++)
        {
            center.definers[i].DrawGizmos(center.position);
        }
        for(int i = 0; i < channels.Count; i++)
        {
            channels[i].DrawGizmos(center.position);
        }
    }

    #region Getters/Setters

    public AreaCenter GetCenter()
    {
        return center;
    }
    public bool HasChannels()
    {
        if (channels.Count == 0) return false;
        return true;
    }

    #endregion
}
