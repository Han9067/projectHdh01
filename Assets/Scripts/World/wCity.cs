using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class wCity : MonoBehaviour
{
    public int id;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")){
            PlayerManager.I.currentCity = id;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerManager.I.currentCity = 0;
    }
}
