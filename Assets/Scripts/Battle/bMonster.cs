using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using DG.Tweening;
public class bMonster : MonoBehaviour
{
    public int objId, monsterId;
    public MonData monData;
    public GameObject shdObj, bodyObj, ggParent, ggObj;
    bool isGG = false;
    public float hp, maxHp;
    public int att, def, crt, crtRate, hit, eva, gainExp;
    public int w, h;
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
        maxHp = monData.HP;
        hp = maxHp;
        att = monData.Att;
        def = monData.Def;
        crt = monData.Crt;
        crtRate = monData.CrtRate;
        hit = monData.Hit;
        eva = monData.Eva;
        gainExp = monData.GainExp;
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
        int ly = y * 100;
        shdObj.GetComponent<SpriteRenderer>().sortingOrder = ly;
        bodyObj.GetComponent<SpriteRenderer>().sortingOrder = ly + 1;
    }
    public void OnDamaged(int dmg, BtFaction attacker)
    {
        dmg = dmg > def ? dmg - def : 0;
        hp -= dmg;
        if (hp > 0 && !isGG)
        {
            ggParent.SetActive(true);
            isGG = true;
        }

        if (hp <= 0)
        {
            StartCoroutine(DeathMon(attacker));
        }
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
}
