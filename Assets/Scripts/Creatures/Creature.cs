using UnityEngine;

public class Creature : MonoBehaviour
{
    public CreatureID id;

    private Animator animator;

    #region Movement
    [Header("Movement")]
    public bool showGizmos;
    [SerializeField] private bool disableMovement;
    [SerializeField] private CreatureMovementBehaviour movement;
    [SerializeField] private Area bounds;

    [Tooltip("For Procedural Animation")]
    [SerializeField] private Transform headTransform;
    #endregion

    void OnEnable()
    {
        InitializeMovement();

        if(id == CreatureID.MauveStinger)
        {
            animator = GetComponent<Animator>();
            animator.speed = 0f;
            animator.Play("Swimming", 0, Random.Range(0, 1f));
            animator.speed = 1f;
        }
    }

    
    void Update()
    {
        if(!disableMovement) movement?.UpdateMovement();
    }

    #region Movement

    private void InitializeMovement()
    {
        if(movement != null)
        {
            CreatureMovementBehaviour myMovement = Instantiate(movement);
            movement = myMovement;

            movement.InitializeMovement(this);
        }
    }

    #endregion

    #region Picture Scoring

    [Header("Scoring")]
    [SerializeField] private float minAverageRaycastValue = 50f;
    [SerializeField] private Vector2 oneStarRange;
    [SerializeField] private Vector2 twoStarRange;
    [SerializeField] private Vector2 threeStarRange;
    public int GetStarRating(float raycastHitPercentage)
    {
        raycastHitPercentage *= 100;

        // 3 Star Rating
        if (threeStarRange.x < raycastHitPercentage && raycastHitPercentage < threeStarRange.y) return 3;

        // 2 Star Rating
        if (twoStarRange.x < raycastHitPercentage && raycastHitPercentage < twoStarRange.y) return 2;

        // 1 Star Rating
        if (oneStarRange.x < raycastHitPercentage && raycastHitPercentage < oneStarRange.y) return 1;

        // Considered not on the picture
        return 0;
    }
    public float GetMinAverageRaycastValue()
    {
        return minAverageRaycastValue;
    }
    #endregion

    #region Getters/Setters
    public bool IsBoid()
    {
        if (movement is BoidMovement)
            return true;
        return false;
    }
    public BoidMovement GetBoidMovement()
    {
        if (movement is BoidMovement) return movement as BoidMovement;

        Debug.LogError("WRONG WRONG WRONG, MOVEMENT ISN'T BOID MOVEMENT, WHY ARE YOU TRYING THIS");

        return null;
    }
    public Transform GetHeadTransform()
    {
        return headTransform;
    }
    public Area GetBounds()
    {
        return bounds;
    }
    #endregion
}
