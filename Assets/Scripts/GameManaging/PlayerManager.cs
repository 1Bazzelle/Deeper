using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerObject;
    [SerializeField] private Player player;
    [SerializeField] private GameObject researchStation;
    [SerializeField] private Transform startingPoint;

    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        #endregion
    }

    public void SubmergePlayer()
    {
        researchStation.SetActive(false);

        player.transform.position = startingPoint.position;
        player.transform.rotation = startingPoint.rotation;

        player.SetMode(Player.PlayerMode.Cockpit);

        playerObject.SetActive(true);
    }
    public void EmergePlayer()
    {
        playerObject.SetActive(false);
        researchStation.SetActive(true);
    }
    public Player GetPlayer()
    {
        return player;
    }
    public Transform GetPlayerTransform()
    {
        return player.transform;
    }
    public ZoneID GetCurZone()
    {
        return Constants.GetCurZoneID(Mathf.Abs(playerObject.transform.position.y));
    }

    public Transform GetResearchBaseTransform()
    {
        return researchStation.transform;
    }
}
