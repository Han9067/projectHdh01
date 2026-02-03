using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Rendering;
using DG.Tweening;
public class bPlayer : MonoBehaviour
{
    public int objId = 1000;
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain, bodyObj;
    public PlayerData pData;
    private Vector3 backupPos;
    Tween hitTween;
    private MaterialPropertyBlock pProp;
    [SerializeField] private SortingGroup sGrp;
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
        hitTween = DOTween.Sequence()
            .Append(transform.DOLocalMove(hitPos, 0.15f).SetEase(Ease.InSine))
            .Append(transform.DOLocalMove(backupPos, 0.1f).SetEase(Ease.InQuad))
            .SetAutoKill(true)
            .OnKill(() =>
            {
                hitTween = null;
            });
    }
    public void SetOutline()
    {
        // foreach (var spr in ptSpr)
        // {
        //     spr.Value.GetPropertyBlock(pProp);
        //     pProp.SetFloat("_Outline", 1f);
        //     pProp.SetColor("_OutlineColor", Color.red);
        //     pProp.SetFloat("_OutlineSize", 10);
        //     spr.Value.SetPropertyBlock(pProp);
        //     spr.Value.color = Color.red;
        // }
    }
    #region ==== 🎨 ORDERING IN LAYER ====
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    #endregion
}
