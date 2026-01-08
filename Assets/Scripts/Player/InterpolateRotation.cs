using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class InterpolateRotation : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // Reference to the CameraPosition
    [SerializeField] private float rotationSpeed = 2.0f; // Speed of rotation towards the camera

    // /*
    void Update()
    {
        if (cameraTransform != null)
        {
            // Get the target rotation from the camera position
            Quaternion targetRotation = cameraTransform.rotation;

            // Smoothly interpolate the submarine's rotation towards the camera position's rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            Debug.LogWarning("Camera Position is not assigned!", this);
        }
    }
    //*/
}
