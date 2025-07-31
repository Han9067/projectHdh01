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
    [SerializeField] private GameObject frm;
    void Start()
    {
        frm.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("icon_human");
        //플레이어의 아이콘 오브젝와 캐릭터 파츠를 적용하고 캐릭터 파츠는 아이콘을 벗어나면 안되니 마스크 기능을 적용해야함
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if(CityEnterPop.isActive)return;
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

        if(PlayerManager.I.currentCity > 0){
            UIManager.ShowPopup("CityEnterPop");
            GB.Presenter.Send("CityEnterPop","EnterCity", PlayerManager.I.currentCity);
        }
    }
}
