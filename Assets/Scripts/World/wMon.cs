using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;

public class wMon : MonoBehaviour
{
    public int monIdx; //배열 인덱스
    [SerializeField] private int monId;
    public List<int> monGrp = new List<int>();
    public SpriteRenderer FrmSpr;
    public SpriteRenderer MainSpr;
    public MonData[] monData;
    void Start()
    {
        FrmSpr.sprite = ResManager.GetSprite(monId < 1000 ? "icon_mon" : "icon_boss");
        MainSpr.sprite = ResManager.GetSprite("mIcon_" + monId);
        monData = new MonData[monGrp.Count];
        for(int i = 0; i < monGrp.Count; i++){
            monData[i] = MonManager.I.MonDataList[monGrp[i]].Clone();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")){
            // Debug.Log("Monster");
            wPlayer player = other.GetComponent<wPlayer>();
            player.stopPlayer();
            //몬스터 매니저를 통해 주변 몬스터 그룹을 찾고 해당 그룹에 존재하는 몬스터 리스트들을 모두 한 리스트에 저장한다음 전투 정보 팝업에 전달하고 그대로 정보를 소유한다.
            //소유한 리스트는 전투 씬으로 넘어가면 해당 데이터를 참고하여 몬스터들을 배치하고 초기화한다.
            // List<int> result = MonManager.I.GetAroundMon(other.transform.position.x, other.transform.position.y, monIdx, monGrp);
            // Debug.Log(result.Count);
            // foreach(var m in result)
            // {
            //     Debug.Log(m);
            // }
            // UIManager.ShowPopup("BattleInfoPop");
            // GB.Presenter.Send("BattleInfoPop","MonInfo", monParty);
        }
    }
}
