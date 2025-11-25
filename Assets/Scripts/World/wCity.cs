using UnityEngine;
using UnityEngine.EventSystems;
using GB;

public class wCity : MonoBehaviour
{
    public int id;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerManager.I.currentCity = id;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerManager.I.currentCity = 0;
    }
    private void OnMouseEnter()
    {
        WorldCore.intoCity = id;
    }
    private void OnMouseExit()
    {
        WorldCore.intoCity = 0;
    }
}
