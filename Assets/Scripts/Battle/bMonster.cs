using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using DG.Tweening;
using UnityEngine.Rendering;
public class bMonster : MonoBehaviour
{
    public int objId, monsterId;
    public string mName;
    public MonData monData;
    public GameObject shdObj, bodyObj, ggParent, ggObj;
    bool isGG = false;
    public float hp, maxHp;
    public int att, def, crt, crtRate, hit, eva, gainExp, lv;
    public int w, h;
    [SerializeField] private SpriteRenderer bodySpr;
    private MaterialPropertyBlock mProp;
    public bool isOutline = false;
    private Color redColor = new Color(1, 0.5f, 0.5f, 1);
    [SerializeField] private SortingGroup sGrp;
    void Start()
    {
        monData = MonManager.I.MonDataList[monsterId].Clone();
        w = monData.W;
        h = monData.H;
        bodyObj.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("mon_" + monsterId);
        shdObj.transform.localScale = new Vector3(monData.SdwScr, monData.SdwScr, 1);
        bodyObj.transform.localPosition = new Vector3((w - 1) * 0.6f, 0.4f, 0);
        shdObj.transform.localPosition = new Vector3((w - 1) * 0.6f, -0.35f, 0);
        ggParent.SetActive(false);
        ggParent.transform.localPosition = new Vector3((w - 1) * 0.6f, monData.GgY, 0);
        mName = monData.Name;
        maxHp = monData.HP;
        hp = maxHp;
        att = monData.Att;
        def = monData.Def;
        crt = monData.Crt;
        crtRate = monData.CrtRate;
        hit = monData.Hit;
        eva = monData.Eva;
        gainExp = monData.GainExp;
        lv = monData.Lv;
        mProp = new MaterialPropertyBlock();
    }
    public void SetMonData(int objId, int monId, float px, float py)
    {
        this.objId = objId;
        this.monsterId = monId;
        transform.position = new Vector3(px, py, 0);
        //w에 따라 내부 자식 리소스 x좌표 변경
    }
    public void SetDirObj(int dir)
    {
        transform.localScale = new Vector3(dir, 1, 1);
        int gDir = dir == -1 ? -1 : 1;
        ggParent.transform.localScale = new Vector3(gDir, 1, 1);
    }
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    public void OnDamaged(int att, int crt, int crtRate, BtFaction attacker)
    {
        Debug.Log("OnDamaged: " + att + " " + crt + " " + crtRate);
        int dmg = GsManager.I.GetDamage(att, def);
        hp -= dmg;
        if (hp > 0 && !isGG)
        {
            ggParent.SetActive(true);
            isGG = true;
        }

        if (hp <= 0)
            StartCoroutine(DeathMon(attacker));
        else
            ggObj.transform.localScale = new Vector3(hp / maxHp, 1, 1);
        //텍스트 연출
        BattleCore.I.ShowDmgTxt(dmg, transform.position); // 데미지 텍스트 표시
    }
    private IEnumerator DeathMon(BtFaction attacker)
    {
        BattleCore.I.DeathObj(objId, attacker);
        //몬스터 죽음 연출
        ggParent.SetActive(false);
        bodyObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        shdObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        //경험치 획득
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
    public void StateOutline(bool on)
    {
        isOutline = on;
        bodySpr.GetPropertyBlock(mProp);
        mProp.SetFloat("_Outline", on ? 1f : 0);
        mProp.SetColor("_OutlineColor", Color.red);
        mProp.SetFloat("_OutlineSize", 10);
        bodySpr.SetPropertyBlock(mProp);
        bodySpr.color = on ? redColor : Color.white;
    }
}
