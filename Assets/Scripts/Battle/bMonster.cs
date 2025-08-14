using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using DG.Tweening;
public class bMonster : MonoBehaviour
{
    public int objId;
    public int monsterId;
    public MonData monData;
    public GameObject shdObj, bodyObj, ggParent, ggObj;
    bool isGG = false;
    public float hp, maxHp;
    public int x, y;
    void Start()
    {
        monData = MonManager.I.MonDataList[monsterId].Clone();
        bodyObj.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("mon_" + monsterId);
        shdObj.transform.localScale = new Vector3(monData.SdwScr, monData.SdwScr, 1);
        shdObj.transform.localPosition = new Vector3(0, monData.SdwY, 0);
        bodyObj.transform.localPosition = new Vector3(0, monData.OffY, 0);
        ggParent.SetActive(false);
        maxHp = monData.HP;
        hp = maxHp;
    }
    public void SetMonData(int objId, int monId, int x, int y, float px, float py)
    {
        this.objId = objId;
        this.monsterId = monId;
        this.x = x;
        this.y = y;
        transform.position = new Vector3(px, py, 0);
        SetObjLayer(y);
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
    public void OnDamaged(int dmg)
    {
        hp -= dmg;
        if (hp > 0 && !isGG)
        {
            ggParent.SetActive(true);
            isGG = true;
        }

        if (hp <= 0)
            StartCoroutine(DeathMon());
        else
            ggObj.transform.localScale = new Vector3(hp / maxHp, 1, 1);
        //텍스트 연출
    }
    private IEnumerator DeathMon()
    {
        //몬스터 죽음 연출
        ggParent.SetActive(false);
        bodyObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        shdObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        //경험치 획득

        //아이템 드롭

        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
