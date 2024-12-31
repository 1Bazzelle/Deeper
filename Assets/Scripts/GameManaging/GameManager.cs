using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;
    private GameManager() { }
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    [SerializeField] private GameObject sub;
    [SerializeField] private Transform subPos;
    [SerializeField] private GameObject researchStationView;

    private GameState curState;

    void Start()
    {
        curState = new ResearchBase();
        curState.EnterState(this);
    }

    void Update()
    {
        curState.UpdateState(this);
    }

    public void ChangeState(GameState newState)
    {
        curState.ExitState(this);
        curState = newState;
        curState.EnterState(this);
    }

    public void ActivateSub(Transform newSubPos)
    {
        subPos = newSubPos;
        subPos.rotation = Quaternion.identity;

        sub.SetActive(true);
    }

    public void DeactivateSub()
    {
        sub.SetActive(false);
    }
}
