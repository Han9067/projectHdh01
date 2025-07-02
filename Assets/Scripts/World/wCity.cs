using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class wCity : MonoBehaviour
{
    public int id;
    // private bool isPlayerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")){
            // isPlayerInside = true;
            WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
            worldMainUI.stateGameSpd("x0");
            
            // 플레이어 스크립트의 stopPlayer() 호출
            wPlayer player = other.GetComponent<wPlayer>();
            if (player != null)
                player.stopPlayer();

            UIManager.ShowPopup("CityEnterPop");
            GB.Presenter.Send("CityEnterPop","EnterCity", id);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // if (other.CompareTag("Player"))
            // isPlayerInside = false;
    }
}
