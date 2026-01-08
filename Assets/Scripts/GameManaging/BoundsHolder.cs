using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoundsHolder : MonoBehaviour
{
    [SerializeField] private List<Area> areas;

    void OnDrawGizmos()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            areas[i].DrawGizmos();
        }
    }
}
