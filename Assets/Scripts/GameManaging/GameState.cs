using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState
{
    abstract public void EnterState(GameManager gameManager);
    abstract public void UpdateState(GameManager gameManager);
    abstract public void ExitState(GameManager gameManager);
}

public class Submerged : GameState
{
    public override void EnterState(GameManager gameManager)
    {
        
    }
    public override void UpdateState(GameManager gameManager)
    {

    }
    public override void ExitState(GameManager gameManager)
    {
        
    }
}
public class ResearchBase : GameState
{
    public override void EnterState(GameManager gameManager)
    {
        
    }
    public override void UpdateState(GameManager gameManager)
    {

    }
    public override void ExitState(GameManager gameManager)
    {

    }
}
