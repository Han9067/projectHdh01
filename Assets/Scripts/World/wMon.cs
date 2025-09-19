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
    [SerializeField] private SpriteRenderer frmBack, frmFront, frmDeco, mainSpr;
    public MonData[] monData;
    [SerializeField] private int uId;
    void Start()
    {

        if (frmBack.GetComponent<SpriteRenderer>().sprite == null)
            frmBack.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_back");
        if (frmFront.GetComponent<SpriteRenderer>().sprite == null)
            frmFront.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_front");
        int mType = monId < 1000 ? 0 : 1;
        float[] rgb = mType == 0 ? new float[] { 112 / 255f, 122 / 255f, 92 / 255f } : new float[] { 180 / 255f, 50 / 255f, 60 / 255f };
        frmBack.GetComponent<SpriteRenderer>().color = new Color(rgb[0], rgb[1], rgb[2], 1);
        frmFront.GetComponent<SpriteRenderer>().color = new Color(rgb[0], rgb[1], rgb[2], 1);
        mainSpr.sprite = ResManager.GetSprite("mon_" + monId);
        monData = new MonData[monGrp.Count];
        for (int i = 0; i < monGrp.Count; i++)
        {
            monData[i] = MonManager.I.MonDataList[monGrp[i]].Clone();
        }
    }

    public void SetMonData(int uid, int mId, List<int> mList)
    {
        uId = uid;
        monId = mId;
        monGrp = mList;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // wPlayer player = other.GetComponent<wPlayer>();
            // player.stopPlayer();
            WorldCore.I.stopPlayer();
            string result = MonManager.I.GetAroundMon(other.transform.position.x, other.transform.position.y, monIdx, monGrp);

            UIManager.ShowPopup("BattleInfoPop");
            Presenter.Send("BattleInfoPop", "MonInfo", result);
        }
    }
}
