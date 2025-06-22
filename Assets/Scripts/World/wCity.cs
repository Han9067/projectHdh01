using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class wCity : MonoBehaviour
{
    public int id;
    private bool isPlayerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")){
            isPlayerInside = true;
            WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
            worldMainUI.stateGameSpd("x0");
            UIManager.ShowPopup("CityEnterPop");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInside = false;
    }
}
