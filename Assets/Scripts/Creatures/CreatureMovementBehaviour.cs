using UnityEngine;


public abstract class CreatureMovementBehaviour : ScriptableObject
{
    protected Creature creature;
    protected Transform target;
    protected MovementState curState;

    public abstract void InitializeMovement(Creature origin);
    public abstract void UpdateMovement();

    public void ChangeMovementState(MovementState newState, Transform _target)
    {
        target = _target;
        curState = newState;
    }

    public enum MovementState
    {
        Idle,
        Standard,
        Fleeing,
        Curious
    }
}