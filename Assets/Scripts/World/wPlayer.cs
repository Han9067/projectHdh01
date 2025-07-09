using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class wPlayer : MonoBehaviour
{
    
    private float speed = 10f;
    private Vector2 targetPosition;
    private bool isMoving = false;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            // Debug.Log("CityEnterPop.isActive: " + CityEnterPop.isActive);
            if(CityEnterPop.isActive)return;
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isMoving = true;

            WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
            worldMainUI.stateGameSpd("x1");
        }

        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                stopPlayer();
            }
        }
    }
    public void stopPlayer()
    {
        isMoving = false;
        WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
        worldMainUI.stateGameSpd("x0");
    }
}
