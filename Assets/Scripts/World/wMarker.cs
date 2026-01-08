using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class wMarker : MonoBehaviour
{
    [SerializeField] private int mkUid, mkType;
    [SerializeField] private SpriteRenderer mainSpr;
    [SerializeField] private SpriteRenderer iconSpr;
    void Start()
    {
        switch (mkType)
        {
            case 99:
                mainSpr.color = Color.yellow;
                iconSpr.sprite = ResManager.GetSprite("mark_e");
                break;
        }
    }
    public void SetMarkerData(int id, int type)
    {
        mkUid = id; //마커 아이디디
        mkType = type;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (mkType)
            {
                case 99:
                    WorldCore.I.StopPlayer();
                    Debug.Log("OnTriggerEnter2D");
                    WorldObjManager.I.TutoMon();
                    UIManager.ShowPopup("BattleReadyPop");
                    Presenter.Send("BattleReadyPop", "MonInfo", "1");
                    break;
            }
        }
    }
}
