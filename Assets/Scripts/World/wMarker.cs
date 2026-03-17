using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class wMarker : MonoBehaviour
{
    [SerializeField] private int mkUid, mkType;
    [SerializeField] private SpriteRenderer spr;
    void Start()
    {
        switch (mkType)
        {
            case 1:
                spr.sprite = ResManager.GetSprite("mark_qst");
                break;
            case 2:
                spr.sprite = ResManager.GetSprite("mark_ran");
                break;
            case 3:
                spr.sprite = ResManager.GetSprite("mark_exp");
                break;
            case 4:
                spr.sprite = ResManager.GetSprite("mark_boss");
                break;
            case 999:
                spr.sprite = ResManager.GetSprite("mark_qst");
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
