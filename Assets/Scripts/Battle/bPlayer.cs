using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Rendering;
using DG.Tweening;
using System.Linq;
public class bPlayer : MonoBehaviour
{
    public int objId = 1000;
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain, bodyObj;
    public PlayerData pData;
    private Vector3 backupPos;
    Tween pbt, hft; //pushBackTween, hitFlashTween
    private Dictionary<PtType, Material> material = new Dictionary<PtType, Material>();
    [SerializeField] private SortingGroup sGrp;

    private static readonly int HitColorID = Shader.PropertyToID("_HitColor"); //HitColorID
    private static readonly int HitAmountID = Shader.PropertyToID("_HitAmount"); //HitAmountID
    private static readonly int OutlineID = Shader.PropertyToID("_Outline"); //OutlineID
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor"); //OutlineColorID
    private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize"); //OutlineSizeID

    private MaterialPropertyBlock pProp; //MaterialPropertyBlock
    private float curHitAmount; //현재 Hit Amount

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
    }
    public void OnDamaged(int dmg, Vector3 pos)
    {
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
