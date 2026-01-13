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
            case 1:
                mainSpr.color = Color.white;
                iconSpr.sprite = ResManager.GetSprite("mark_e");
                break;
            case 999:
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
            WorldCore.I.StopPlayer();
            switch (mkType)
            {
                case 1:
                    UIManager.ShowPopup("EventPop");
                    Presenter.Send("EventPop", "SetEvent", new List<int> { mkType, mkUid });
                    break;
                case 999:
                    WorldObjManager.I.TutoMon();
                    UIManager.ShowPopup("BattleReadyPop");
                    Presenter.Send("BattleReadyPop", "MonInfo", "1");
                    break;
            }
        }
    }
}
