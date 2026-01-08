using UnityEngine;

public class StateMainMenu : GameState
{
    public override void EnterState()
    {
        if (UIManager.Instance != null) UIManager.Instance.ChangeScreen(UIManager.ScreenID.MainMenu);
        PlayerManager.Instance.EmergePlayer();
    }
    public override void UpdateState()
    {

    }
    public override void ExitState()
    {

    }
}
