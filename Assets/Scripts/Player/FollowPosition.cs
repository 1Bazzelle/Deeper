using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    [SerializeField] private Transform followTransform;

    [Header("Offsets")]
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;
    [SerializeField] private float zOffset;
    private void Update()
    {
        transform.position = followTransform.position;

        transform.position = new Vector3(followTransform.position.x + xOffset, followTransform.position.y + yOffset, followTransform.position.z + zOffset);
    }
}
