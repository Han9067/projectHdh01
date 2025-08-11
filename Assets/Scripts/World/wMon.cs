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
        MainSpr.sprite = ResManager.GetSprite("mon_" + monId);
        monData = new MonData[monGrp.Count];
        for (int i = 0; i < monGrp.Count; i++)
        {
            monData[i] = MonManager.I.MonDataList[monGrp[i]].Clone();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            wPlayer player = other.GetComponent<wPlayer>();
            player.stopPlayer();
            string result = MonManager.I.GetAroundMon(other.transform.position.x, other.transform.position.y, monIdx, monGrp);
            Debug.Log(result);

            UIManager.ShowPopup("BattleInfoPop");
            GB.Presenter.Send("BattleInfoPop", "MonInfo", result);
        }
    }
}
