using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wCity : MonoBehaviour
{
    public int id;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(id + " 마을에서 플레이어 감지");
    }
}
