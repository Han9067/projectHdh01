using System.Collections.Generic;
using UnityEngine;
using GB;
using DG.Tweening;
using UnityEngine.Rendering;

public class wMon : MonoBehaviour
{
    public int monIdx, areaID; //배열 인덱스
    [SerializeField] private int monId;
    public List<int> monGrp = new List<int>();
    [SerializeField] private SpriteRenderer frmBack, frmFront, frmDeco, mainSpr, overSpr;
    public SpriteRenderer traceSpr;
    public MonData[] monData;
    [SerializeField] private int uId;
    [SerializeField] private SortingGroup sGrp;
    public bool isOutline = false;
    private float alpha = 1f;
    public Vector3 tgPos, myPos;
    public float mSpd = 0.4f; //몬스터 이동 속도
    public List<Vector3> path = new List<Vector3>();
    public int pathIdx = 0;
    void Start()
    {
        sGrp = GetComponent<SortingGroup>();
        if (frmBack.GetComponent<SpriteRenderer>().sprite == null)
            frmBack.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_back");
        if (frmFront.GetComponent<SpriteRenderer>().sprite == null)
            frmFront.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_front");
        int mType = monId < 1000 ? 0 : 1;
        float[] rgb = mType == 0 ? new float[] { 112 / 255f, 122 / 255f, 92 / 255f } : new float[] { 180 / 255f, 50 / 255f, 60 / 255f };
        frmBack.GetComponent<SpriteRenderer>().color = new Color(rgb[0], rgb[1], rgb[2], alpha);
        frmFront.GetComponent<SpriteRenderer>().color = new Color(rgb[0], rgb[1], rgb[2], alpha);
        mainSpr.sprite = ResManager.GetSprite("mIcon_" + monId);
        monData = new MonData[monGrp.Count];
        for (int i = 0; i < monGrp.Count; i++)
            monData[i] = MonManager.I.MonDataList[monGrp[i]].Clone();
        overSpr.gameObject.SetActive(false);
        traceSpr.gameObject.SetActive(false);
    }
    public void SetObjLayer(float y)
    {
        sGrp.sortingOrder = (int)((80 - y) * 10);
    }
    public void SetMonData(int uid, int area, int mId, List<int> mList)
    {
        uId = uid;
        areaID = area;
        monId = mId;
        monGrp = mList;
    }

    public void TraceObj(bool on)
    {
        traceSpr.gameObject.SetActive(on);
    }
    public void OverObj(bool on)
    {
        overSpr.gameObject.SetActive(on);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            WorldCore.I.StopPlayer();
            string result = WorldObjManager.I.GetAroundMon(monGrp, uId, other.transform.position.x, other.transform.position.y, monIdx);

            UIManager.ShowPopup("BattleReadyPop");
            Presenter.Send("BattleReadyPop", "MonInfo", result);
        }
    }
    public void SetActiveTween(bool isActive, int type)
    {
        SpriteRenderer[] arr = { frmBack, frmFront, frmDeco, mainSpr };
        alpha = isActive ? 0f : 1f;
        foreach (var s in arr)
        {
            Color c = s.color;
            c.a = alpha;
            s.color = c;
        }
        switch (type)
        {
            case 0:
                gameObject.SetActive(isActive);
                break;
            case 1:
                int cnt = 0;
                float tgAlpha = isActive ? 1f : 0f;
                if (isActive)
                    gameObject.SetActive(true);
                foreach (var s in arr)
                {
                    s.DOFade(tgAlpha, 0.5f).SetUpdate(true)
                    .OnComplete(() =>
                    {
                        if (!isActive)
                        {
                            cnt++;
                            if (cnt == 4) gameObject.SetActive(false);
                        }
                    });
                }
                break;
        }
    }
}
