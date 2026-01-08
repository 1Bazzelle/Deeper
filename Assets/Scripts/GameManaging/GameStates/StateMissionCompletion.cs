using UnityEngine;

public class StateMissionCompletion : GameState
{
    public override void EnterState()
    {
        PlayerManager.Instance.EmergePlayer();
        UIManager.Instance.ChangeScreen(UIManager.ScreenID.MissionCompletion);
    }
    public override void UpdateState()
    {

    }
    public override void ExitState()
    {

    }
}
