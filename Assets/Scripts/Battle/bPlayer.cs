using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Rendering;
using DG.Tweening;
using System.Linq;
public class bPlayer : MonoBehaviour
{
    public int objId = 1000;
    private int angIdx = 0; //이동이 회전하며 움직일때 해당 변수는 0 또는 1이 변동되며 해당 값에 따라 왼쪽,오른쪽으로 회전
    public float dir = 1; //바라보는 방향, 1 : 왼쪽, -1 : 오른쪽
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain, bodyObj;
    public PlayerData pData;
    private Vector3 backupPos;
    Tween pbt, hft; //pushBackTween, hitFlashTween
    [SerializeField] private SortingGroup sGrp;
    public BoxCollider2D bColl;
    #region ==== Hit Effect ====
    private static readonly int HitColorID = Shader.PropertyToID("_HitColor"); //HitColorID
    private static readonly int HitAmountID = Shader.PropertyToID("_HitAmount"); //HitAmountID
    private MaterialPropertyBlock pProp; //MaterialPropertyBlock
    private float curHitAmount; //현재 Hit Amount
    #endregion

    void Awake()
    {
        GsManager.I.SetObjParts(ptSpr, ptMain);
        pProp = new MaterialPropertyBlock();
    }
    void Start()
    {
        pData = PlayerManager.I.pData;
        GsManager.I.SetObjAppearance(0, ptSpr);
        GsManager.I.SetObjAllEqParts(0, ptSpr);
    }
    public float GetObjDir()
    {
        return bodyObj.transform.localScale.x;
    }
    public void SetObjDir(float dir)
    {
        bodyObj.transform.localScale = new Vector3(dir, 1, 1);
        this.dir = dir;
    }
    public void OnJump(float dur)
    {
        //ptMain.transform.DOLocalJump(new Vector3(0, 0.1f, 0), jumpPower: 0.3f, numJumps: 1, duration: dur).SetEase(Ease.OutQuad);
        angIdx = angIdx == 0 ? 1 : 0;
        float ang = angIdx == 0 ? Random.Range(-12f, -4f) : Random.Range(4f, 12f);

        DOTween.Sequence()
        .Join(ptMain.transform.DOLocalJump(new Vector3(0, 0.1f, 0), 0.3f, 1, dur))
        .Join(ptMain.transform.DOLocalRotate(new Vector3(0, 0, ang), dur * 0.45f).SetEase(Ease.OutQuad))
        .Append(ptMain.transform.DOLocalRotate(Vector3.zero, dur * 0.2f).SetEase(Ease.OutQuad));
    }
    public void OnDamaged(int dmg, Vector3 pos)
    {
        Presenter.Send("BattleMainUI", "ShowMsg", string.Format(LocalizationManager.GetValue("Msg_Hit"), pData.Name, dmg));
        pData.HP -= dmg;
        if (pData.HP <= 0)
        {
            pData.HP = 0;
            Debug.Log("Player Dead");
        }
        else
            OnHitAction(pos);

        Presenter.Send("BattleMainUI", "GetPlayerHp");
        BattleCore.I.ShowBloodScreen();
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
            spr.Value.GetPropertyBlock(pProp);
            pProp.SetColor(HitColorID, Color.red);
            pProp.SetFloat(HitAmountID, 1f);
            spr.Value.SetPropertyBlock(pProp);
        }

        hft = DOTween.To(
            () => curHitAmount,
            x =>
            {
                curHitAmount = x;
                foreach (var spr in ptSpr)
                {
                    spr.Value.GetPropertyBlock(pProp);
                    pProp.SetFloat(HitAmountID, x);
                    spr.Value.SetPropertyBlock(pProp);
                }
            },
            0f, 0.3f
        ).SetEase(Ease.OutQuad).SetAutoKill(true).OnKill(() => hft = null);
    }
    public void StateOutline()
    {
        foreach (var spr in ptSpr)
        {
            spr.Value.GetPropertyBlock(pProp);
            pProp.SetColor(HitColorID, Color.red);
            // pProp.SetFloat(HitAmountID, on ? 0.5f : 0);
            pProp.SetFloat(HitAmountID, 0.5f);
            spr.Value.SetPropertyBlock(pProp);
        }
    }

    #region ==== 🎨 ORDERING IN LAYER ====
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    #endregion
}
