using System.Collections;
using UnityEngine;
using GB;
using DG.Tweening;
using UnityEngine.Rendering;
public class bMonster : MonoBehaviour
{
    public int objId, monsterId;
    public string mName;
    public MonData monData;
    public GameObject shdObj, mainObj, ggParent, ggObj, bodyObj;
    bool isGG = false;
    public float hp, maxHp;
    public int att, def, crt, crtRate, hit, eva, gainExp, lv;
    public int w, h, Rng;
    [SerializeField] private SpriteRenderer mainSpr;
    private MaterialPropertyBlock mProp;
    public bool isOutline = false;
    private Vector3 backupPos;
    Tween hitTween;
    private Color redColor = new Color(1, 0.5f, 0.5f, 1);
    [SerializeField] private SortingGroup sGrp;
    void Start()
    {
        monData = MonManager.I.MonDataList[monsterId].Clone();
        w = monData.W;
        h = monData.H;
        mainObj.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("mon_" + monsterId);
        shdObj.transform.localScale = new Vector3(monData.SdwScr, monData.SdwScr, 1);
        mainObj.transform.localPosition = new Vector3(0, 0.4f, 0);
        shdObj.transform.localPosition = new Vector3(0, -0.35f, 0);
        ggParent.SetActive(false);
        ggParent.transform.localPosition = new Vector3(0, monData.GgY, 0);
        bodyObj.transform.localPosition = new Vector3((w - 1) * 0.6f, 0, 0);
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
        Rng = monData.Rng;
        mProp = new MaterialPropertyBlock();
    }
    public void SetMonData(int objId, int monId, float px, float py)
    {
        this.objId = objId;
        this.monsterId = monId;
        transform.position = new Vector3(px, py, 0);
        //w에 따라 내부 자식 리소스 x좌표 변경
    }
    public float GetObjDir()
    {
        return bodyObj.transform.localScale.x;
    }
    public void SetObjDir(float dir)
    {
        bodyObj.transform.localScale = new Vector3(dir, 1, 1);
    }
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    public void OnDamaged(int dmg, BtFaction attacker, Vector3 pos)
    {
        hp -= dmg;
        if (hp > 0 && !isGG)
        {
            ggParent.SetActive(true);
            isGG = true;
        }

        if (hp <= 0)
            StartCoroutine(DeathMon(attacker));
        else
        {
            ggObj.transform.localScale = new Vector3(hp / maxHp, 1, 1);
            //피격에 대한 액션
            OnHitAction(pos);
        }
    }
    private void OnHitAction(Vector3 pos)
    {
        if (hitTween != null)
        {
            hitTween.Kill();
            transform.position = backupPos;
        }

        Vector3 worldDir = transform.position - pos;
        worldDir.z = 0f;  // 2D면 Z 무시
        worldDir.Normalize();

        float pushBack = 0.4f;

        Vector3 localOffset = transform.InverseTransformDirection(worldDir) * pushBack;
        Vector3 hitPos = transform.position + localOffset;
        backupPos = transform.position;
        StateOutline(true);
        hitTween = DOTween.Sequence()
            .Append(transform.DOLocalMove(hitPos, 0.15f).SetEase(Ease.InSine))
            .Append(transform.DOLocalMove(backupPos, 0.1f).SetEase(Ease.InQuad))
            .SetAutoKill(true)
            .OnKill(() =>
            {
                hitTween = null;
                StateOutline(false);
            });
    }
    private IEnumerator DeathMon(BtFaction attacker)
    {
        BattleCore.I.DeathObj(objId, attacker);
        //몬스터 죽음 연출
        ggParent.SetActive(false);
        mainObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        shdObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        //경험치 획득
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
    public void StateOutline(bool on)
    {
        isOutline = on;
        mainSpr.GetPropertyBlock(mProp);
        mProp.SetFloat("_Outline", on ? 1f : 0);
        mProp.SetColor("_OutlineColor", Color.red);
        mProp.SetFloat("_OutlineSize", 10);
        mainSpr.SetPropertyBlock(mProp);
        mainSpr.color = on ? redColor : Color.white;
    }
}
