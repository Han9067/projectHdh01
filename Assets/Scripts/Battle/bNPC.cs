using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Rendering;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;

public class bNPC : MonoBehaviour
{
    public int objId = 0, npcId = 0;
    private int angIdx = 0; //이동이 회전하며 움직일때 해당 변수는 0 또는 1이 변동되며 해당 값에 따라 왼쪽,오른쪽으로 회전
    public float dmgPosY = 1f; //데미지 포지션 Y
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain, bodyObj, ggParent, ggObj;
    public NpcData nData;
    private Vector3 backupPos;
    Tween pbt, hft; //pushBackTween, hitFlashTween
    [SerializeField] private SortingGroup sGrp;
    private MaterialPropertyBlock nProp;
    private float curHitAmount; //현재 Hit Amount
    public bool isOutline = false;
    bool isGG = false;
    //-------------------------------
    public int rng = 0, atkType = 0;
    public string nName;
    public float hp, maxHp, mp, maxMp, sp, maxSp;
    public int att, mAtt, def, mDef, crt, crtRate, hit, eva, gainExp, lv;
    void Awake()
    {
        GsManager.I.SetObjParts(ptSpr, ptMain);
        nProp = new MaterialPropertyBlock();
        ggParent.SetActive(false);
    }
    void Start()
    {
        GsManager.I.SetObjAppearance(npcId, ptSpr);
        GsManager.I.SetObjAllEqParts(npcId, ptSpr);
        rng = nData.Rng;
        atkType = nData.AtkType;
        nName = nData.Name;
        hp = nData.HP;
        maxHp = nData.MaxHP;
        mp = nData.MP;
        maxMp = nData.MaxMP;
        sp = nData.SP;
        maxSp = nData.MaxSP;
        att = nData.Att;
        mAtt = nData.MAtt;
        def = nData.Def;
        mDef = nData.MDef;
        crt = nData.Crt;
        crtRate = nData.CrtRate;
        hit = nData.Hit;
        eva = nData.Eva;
        gainExp = nData.GainExp;
        lv = nData.Lv;
    }
    public void SetNpcData(int npcId, float px, float py)
    {
        this.npcId = npcId;
        transform.position = new Vector3(px, py, 0);
        nData = NpcManager.I.NpcDataList[npcId];
    }
    public float GetObjDir()
    {
        return bodyObj.transform.localScale.x;
    }
    public void SetObjDir(float dir)
    {
        bodyObj.transform.localScale = new Vector3(dir, 1, 1);
    }
    public void OnJump(float dur)
    {
        angIdx = angIdx == 0 ? 1 : 0;
        float ang = angIdx == 0 ? Random.Range(-12f, -4f) : Random.Range(4f, 12f);

        DOTween.Sequence()
        .Join(ptMain.transform.DOLocalJump(new Vector3(0, 0.1f, 0), 0.3f, 1, dur))
        .Join(ptMain.transform.DOLocalRotate(new Vector3(0, 0, ang), dur * 0.45f).SetEase(Ease.OutQuad))
        .Append(ptMain.transform.DOLocalRotate(Vector3.zero, dur * 0.2f).SetEase(Ease.OutQuad));
    }
    public void OnDamaged(int dmg, BtFaction attacker, Vector3 pos)
    {
        if (hp <= 0) return; //죽은 몬스터는 데미지를 받지 않음
        if (dmg > 0)
        {
            Presenter.Send("BattleMainUI", "ShowMsg", string.Format(LocalizationManager.GetValue("Msg_Hit"), nName, dmg));
            OnHitAction(pos);
        }
        else
        {
            Presenter.Send("BattleMainUI", "ShowMsg", string.Format(LocalizationManager.GetValue("Msg_Miss"), nName));
            return;
        }

        hp -= dmg;
        if (hp > 0 && !isGG)
        {
            ggParent.SetActive(true);
            isGG = true;
        }

        if (hp <= 0)
            StartCoroutine(DeathObj(attacker));
        else
        {
            ggObj.transform.localScale = new Vector3(hp / maxHp, 1, 1);
            //피격에 대한 액션
            OnHitAction(pos);
        }
    }
    private void OnHitAction(Vector3 pos)
    {
        pushBackObj(pos);
        hitFlashObj();
    }
    private void pushBackObj(Vector3 pos)
    {
        if (pbt != null)
        {
            pbt.Kill();
            transform.position = backupPos;
        }

        Vector3 worldDir = transform.position - pos;
        worldDir.z = 0f;  // 2D면 Z 무시
        worldDir.Normalize();

        float pushBack = 0.4f;

        Vector3 localOffset = transform.InverseTransformDirection(worldDir) * pushBack;
        Vector3 hitPos = transform.position + localOffset;
        backupPos = transform.position;
        pbt = DOTween.Sequence()
            .Append(transform.DOLocalMove(hitPos, 0.15f).SetEase(Ease.InSine))
            .Append(transform.DOLocalMove(backupPos, 0.1f).SetEase(Ease.InQuad))
            .SetAutoKill(true)
            .OnKill(() =>
            {
                pbt = null;
            });
    }
    private void hitFlashObj()
    {
        if (hft != null)
            hft.Kill();

        curHitAmount = 1f;

        // 초기 상태: 흰색 플래시
        foreach (var spr in ptSpr)
        {
            if (spr.Value.gameObject.activeSelf)
                ObjShd.ApplyShd(spr.Value, nProp, Color.white);
        }

        hft = DOTween.To(
            () => curHitAmount,
            x =>
            {
                curHitAmount = x;
                foreach (var spr in ptSpr)
                {
                    if (spr.Value.gameObject.activeSelf)
                        ObjShd.ApplyShd(spr.Value, nProp, Color.white, x);
                }
            },
            0f, 0.3f
        ).SetEase(Ease.OutQuad).SetAutoKill(true).OnKill(() => hft = null);
    }
    private IEnumerator DeathObj(BtFaction attacker)
    {
        BattleCore.I.DeathObj(objId, attacker);
        //NPC 죽음 연출
        ggParent.SetActive(false);
        bodyObj.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f);
        //경험치 획득
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
    public void StateOutline(bool on)
    {
        isOutline = on;
        foreach (var spr in ptSpr)
        {
            if (spr.Value.gameObject.activeSelf)
                ObjShd.ApplyShd(spr.Value, nProp, Color.red, on ? 0.5f : 0f);
        }
    }
    #region ==== 🎨 ORDERING IN LAYER ====
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    #endregion
}
