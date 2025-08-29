using System.Collections;
using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.EventSystems;

public class wPlayer : MonoBehaviour
{

    private float speed = 10f;
    private Vector2 targetPosition;
    private bool isMoving = false;
    [SerializeField] private GameObject frmBack, frmFront;
    void Start()
    {
        if (frmBack.GetComponent<SpriteRenderer>().sprite == null)
            frmBack.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_back");
        if (frmFront.GetComponent<SpriteRenderer>().sprite == null)
            frmFront.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_front");
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (CityEnterPop.isActive) return;
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isMoving = true;

            WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
            worldMainUI.stateGameSpd("X1");
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
        worldMainUI.stateGameSpd("X0");

        if (PlayerManager.I.currentCity > 0)
        {
            UIManager.ShowPopup("CityEnterPop");
            GB.Presenter.Send("CityEnterPop", "EnterCity", PlayerManager.I.currentCity);
        }
    }
}
