using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class wMarker : MonoBehaviour
{
    public int mkUid, eventID, mkType, Grade;
    public Vector3 mkPos;
    public List<int> monList = new List<int>();
    public bool isGuildQst = false;
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
    public void SetMarkerData(int id, int evt, int type, int g, Vector3 pos, List<int> list, bool isGQst)
    {
        mkUid = id; //마커 아이디
        eventID = evt; //이벤트 ID
        mkType = type; //마커 타입
        Grade = g; //마커 등급
        mkPos = pos; //마커 위치
        monList = list; //마커 몬스터 리스트
        isGuildQst = isGQst; //길드 퀘스트 여부
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
