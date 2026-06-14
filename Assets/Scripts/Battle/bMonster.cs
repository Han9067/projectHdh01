using System.Collections;
using UnityEngine;
using GB;
using DG.Tweening;
using UnityEngine.Rendering;
using System.Collections.Generic;
public class bMonster : MonoBehaviour
{
    public int objId, monId;
    private int angIdx = 0;
    public string mName, attKey;
    public MonData monData;
    public GameObject shdObj, mainObj, ggParent, ggObj, bodyObj;
    bool isGG = false;
    public float hp, maxHp;
    public float dir = 1, dmgPosY = 1f;
    public int att, mAtt, def, mDef, crt, crtRate, hit, eva, gainExp, lv;
    public int w, h, rng, atkType, attId = 0;
    [SerializeField] private SpriteRenderer mainSpr;
    public bool isOutline = false;
    private Vector3 backupPos, backupBodyPos;
    [SerializeField] private SortingGroup sGrp;
    // private Color redColor = new Color(1, 0.5f, 0.5f, 1);
    private MaterialPropertyBlock mProp;
    Tween pbt, hft; //pushBackTween, hitFlashTween
    private float curHitAmount; //현재 Hit Amount
    [Header("Human Preset")]
    private bool isHuman = false;
    private Dictionary<string, int> presetList = new Dictionary<string, int>();
    [SerializeField] private SpriteRenderer faceSpr, handSpr, wp1Spr, wp2Spr;
    // private int idx = 0;
    // public Dictionary<string, int> preset;

    void Awake()
    {
        mProp = new MaterialPropertyBlock();
    }
    void Start()
    {
        monData = MonManager.I.MonDataList[monId].Clone();
        w = monData.W;
        h = monData.H;
        #region 정보 및 능력치 설정
        mName = monData.Name;
        maxHp = monData.HP;
        hp = maxHp;
        att = monData.Att;
        mAtt = monData.MAtt;
        def = monData.Def;
        mDef = monData.MDef;
        crt = monData.Crt;
        crtRate = monData.CrtRate;
        hit = monData.Hit;
        eva = monData.Eva;
        gainExp = monData.GainExp;
        lv = monData.Lv;
        rng = GetAttRng(monData.MonType);
        atkType = GetAttackType(monId);
        attId = GetAttId(monId);
        attKey = GetMonAttKey(monId);
        #endregion

        #region 스프라이트 설정
        string res = $"mon_{monId}";
        if (isHuman)
        {
            List<int> wpList = MonManager.I.GetMonWp(presetList["Weapon"]);
            wp1Spr.sprite = ResManager.GetSprite($"wp{wpList[0]}");
            int hand = 1;
            switch (ItemManager.I.GetItemType(wpList[0]))
            {
                case 12:
                case 14:
                case 16:
                    //양손무기
                    hand = 3;
                    wp1Spr.transform.localPosition = new Vector3(0.38f, 1.045f, 0);
                    wp1Spr.transform.rotation = Quaternion.Euler(0, 0, 25f);
                    break;
                case 19:
                case 20:
                    //창
                    hand = 2;
                    wp1Spr.transform.localPosition = new Vector3(-0.395f, -0.062f, 0);
                    wp1Spr.transform.rotation = Quaternion.Euler(0, 0, 83f);
                    break;
                case 21:
                    //활
                    wp1Spr.transform.localPosition = new Vector3(-0.685f, 0.26f, 0);
                    break;
                default:
                    //한손무기
                    mProp = new MaterialPropertyBlock();
                    wp2Spr.gameObject.SetActive(true);
                    wp2Spr.sprite = ResManager.GetSprite($"wp{wpList[1]}");
                    //추후 2번째 무기가 방패인데 한손 무기인지 체크해줘야함
                    break;
            }
            //mon_63_1
            mainSpr.sprite = ResManager.GetSprite($"{res}_body");
            handSpr.sprite = ResManager.GetSprite($"{res}_{hand}");
            faceSpr.sprite = ResManager.GetSprite($"{res}_face");
        }
        else
        {
            mainSpr.sprite = ResManager.GetSprite(res);
            shdObj.transform.localScale = new Vector3(monData.SdwScr, monData.SdwScr, 1);
            mainObj.transform.localPosition = new Vector3(0, 0.4f, 0);
            shdObj.transform.localPosition = new Vector3((w - 1) * 0.6f, -0.35f, 0);
        }
        ggParent.SetActive(false);
        ggParent.transform.localPosition = new Vector3(0, monData.GgY, 0);
        bodyObj.transform.localPosition = new Vector3((w - 1) * 0.6f, 0, 0);
        backupBodyPos = new Vector3((w - 1) * 0.6f, 0, 0); //이동시 점프 후 백업 위치로 이동 시키기 위한 벡터

        dmgPosY = mainSpr.bounds.size.y * 0.5f;
        #endregion
    }
    public void SetMonNData(int oid, int mId, float px, float py)
    {
        objId = oid;
        monId = mId;
        transform.position = new Vector3(px, py, 0);
        //w에 따라 내부 자식 리소스 x좌표 변경
    }
    public void SetMonHData(int oid, int mId, float px, float py)
    {
        isHuman = true;
        objId = oid;
        monId = mId;
        transform.position = new Vector3(px, py, 0);
        //w에 따라 내부 자식 리소스 x좌표 변경
        presetList = MonManager.I.GetHumanMonPreset(monId);
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
        // bodyObj.transform.DOLocalJump(backupBodyPos, jumpPower: 0.3f, numJumps: 1, duration: dur).SetEase(Ease.OutQuad);
        angIdx = angIdx == 0 ? 1 : 0;
        float ang = angIdx == 0 ? Random.Range(-12f, -4f) : Random.Range(4f, 12f);

        DOTween.Sequence()
        .Join(bodyObj.transform.DOLocalJump(backupBodyPos, 0.3f, 1, dur))
        .Join(bodyObj.transform.DOLocalRotate(new Vector3(0, 0, ang), dur * 0.45f).SetEase(Ease.OutQuad))
        .Append(bodyObj.transform.DOLocalRotate(Vector3.zero, dur * 0.2f).SetEase(Ease.OutQuad));
    }
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    public void OnDamaged(int dmg, BtFaction attacker, Vector3 pos)
    {
        if (hp <= 0) return; //죽은 몬스터는 데미지를 받지 않음
        if (dmg > 0)
        {
            Presenter.Send("BattleMainUI", "ShowMsg", string.Format(LocalizationManager.GetValue("Msg_Hit"), mName, dmg));
            OnHitAction(pos);
        }
        else
        {
            Presenter.Send("BattleMainUI", "ShowMsg", string.Format(LocalizationManager.GetValue("Msg_Miss"), mName));
            return;
        }

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
        ObjShd.ApplyShd(mainSpr, mProp, Color.white);
        if (isHuman)
        {
            ApplyHumanShd(Color.white);
        }
        hft = DOTween.To(
            () => curHitAmount,
            x =>
            {
                curHitAmount = x;
                ObjShd.ApplyShd(mainSpr, mProp, Color.white, x);
                if (isHuman)
                {
                    ApplyHumanShd(Color.white, x);
                }
            }, 0f, 0.3f
        ).SetEase(Ease.OutQuad).SetAutoKill(true).OnKill(() => hft = null);
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
        ObjShd.ApplyShd(mainSpr, mProp, Color.red, on ? 0.5f : 0f);
        if (!isHuman) return;
        ApplyHumanShd(Color.red, on ? 0.5f : 0f);
    }
    private void ApplyHumanShd(Color color, float amount = 1f)
    {
        ObjShd.ApplyShd(faceSpr, mProp, color, amount);
        ObjShd.ApplyShd(handSpr, mProp, color, amount);
        ObjShd.ApplyShd(wp1Spr, mProp, color, amount);
        if (wp2Spr.gameObject.activeSelf)
            ObjShd.ApplyShd(wp2Spr, mProp, color, amount);
    }
    private int GetAttRng(int mType)
    {
        switch (mType)
        {
            case 2:
                int wpId = MonManager.I.GetMonWp(presetList["Weapon"])[0];
                return ItemManager.I.GetWpRng(wpId);
            default:
                return monData.Rng;
        }
    }
    private int GetAttackType(int mId)
    {
        switch (mId)
        {
            case 43:
                return 1;
            case 61:
            case 62:
                int wpId = MonManager.I.GetMonWp(presetList["Weapon"])[0];
                return wpId > 50000 && wpId < 54000 ? 1 : 0;
            default:
                return 0;
        }
    }
    private int GetAttId(int mId)
    {
        switch (mId)
        {
            case 43:
                return 54001;
            case 61:
            case 62:
                int wpId = MonManager.I.GetMonWp(presetList["Weapon"])[0];
                return wpId > 50000 && wpId < 54000 ? 54001 : 0;
            default:
                return 0;
        }
    }
    public string GetMonAttKey(int id)
    {
        switch (id)
        {
            case 61:
            case 62:
            case 63:
            case 81:
            case 82:
            case 83:
                int wpType = ItemManager.I.GetItemType(MonManager.I.GetMonWp(presetList["Weapon"])[0]);
                switch (wpType)
                {
                    case 11:
                    case 12:
                        return "N_Att2";
                    case 13:
                    case 14:
                        return "N_Att3";
                    case 15:
                    case 16:
                        return "N_Att4";
                    case 17:
                    case 18:
                        return "N_Att1";
                    case 19:
                        return "N_Att5";
                    case 20:
                        return "Bow";
                    default:
                        return "N_Att1";
                }
            default:
                return "N_Att1";
        }
    }
}
