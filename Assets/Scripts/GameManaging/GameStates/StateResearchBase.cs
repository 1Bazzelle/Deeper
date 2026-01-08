using UnityEngine;

public class StateResearchBase : GameState
{
    public override void EnterState()
    {
        GameManager.Instance.descent.Setup();

        PlayerManager.Instance.EmergePlayer();

        GameManager.Instance.GenerateMissions();

        UIManager.Instance.ChangeScreen(UIManager.ScreenID.ResearchBase);
    }
    public override void UpdateState()
    {

    }
    public override void ExitState()
    {
        GameManager.Instance.ClearMissions();
    }
}
