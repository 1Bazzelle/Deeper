using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Jellyfish Movement", menuName = "Creature Movement/Jellyfish")]
public class JellyfishMovement : CreatureMovementBehaviour
{
    private Creature myCreature;
    private Rigidbody rb;
    private float baseY;
    private float randomizedOffset;

    [Header("Jellyfish Specific Data")]
    [SerializeField] private float maxInterpolation = 10;
    [SerializeField] private float interpolationSpeed = 2;

    public override void InitializeMovement(Creature origin)
    {
        // Setting MovementState to standard, every creature will initialize with standard movement
        curState = MovementState.Standard;

        myCreature = origin;
        rb = myCreature.GetComponent<Rigidbody>();

        baseY = myCreature.transform.position.y;
        randomizedOffset = Random.Range(0, 12f);
    }

    public override void UpdateMovement()
    {
        switch (curState)
        {
            case MovementState.Standard:

                #region Movement

                rb.linearVelocity = Vector3.zero;

                myCreature.transform.position = new Vector3(
                    myCreature.transform.position.x,
                    baseY + maxInterpolation * Mathf.Sin(interpolationSpeed * Time.time + randomizedOffset), 
                    myCreature.transform.position.z);

                #endregion
                break;
        }
    }
}
