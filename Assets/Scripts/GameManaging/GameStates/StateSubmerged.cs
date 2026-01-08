using UnityEngine;

public class StateSubmerged : GameState
{
    public override void EnterState()
    {
        if(GameManager.Instance.descent.takenPictures == null) 
            GameManager.Instance.descent.Setup();

        if(UIManager.Instance != null) 
            UIManager.Instance.ChangeScreen(UIManager.ScreenID.None);

        PlayerManager.Instance.SubmergePlayer();

        if (SoundManager.Instance != null) 
            SoundManager.Instance.TogglePlayerSubmerged(true);
    }
    public override void UpdateState()
    {
        if (GameManager.Instance.descent.takenPictures.Count >= 1 && Input.GetKeyDown(KeyCode.G))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.Instance.ChangeGameState(new StateMissionCompletion());
        }
    }
    public override void ExitState()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.TogglePlayerSubmerged(false);
        SoundManager.Instance.ResetVoiceLines();
    }
}
