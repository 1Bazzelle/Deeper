using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIEmbark : MonoBehaviour
{
    private bool embarkPossible;
    private bool lastState;

    private Button button;

    void Start()
    {
        embarkPossible = true;
        lastState = embarkPossible;

        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void Update()
    {
        if(embarkPossible != lastState)
        {
            UpdateState(embarkPossible);

            lastState = embarkPossible;
        }
    }

    private void OnClick()
    {
        Debug.Log("Embark Clicked");
        if (embarkPossible)
        {
            Embark();
        }
    }
    public void SetEmbarkState(bool newState)
    {
        embarkPossible = newState;
    }
    private void UpdateState(bool curState)
    {
        if(curState)
        {

        }
        else
        {

        }
    }
    private void Embark()
    {
        Debug.Log("Embarking");
        GameManager.Instance.ChangeState(new Submerged());
    }
}
