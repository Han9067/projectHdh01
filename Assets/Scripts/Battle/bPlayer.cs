using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Rendering;

public class bPlayer : MonoBehaviour
{
    public int objId = 1000;
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain, bodyObj;
    public PlayerData pData;
    [SerializeField] private SortingGroup sGrp;
    void Awake()
    {
        GsManager.I.SetObjParts(ptSpr, ptMain);
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
    public void OnDamaged(int dmg)
    {
        pData.HP -= dmg;
        if (pData.HP <= 0)
        {
            pData.HP = 0;
            Debug.Log("Player Dead");
        }
        Presenter.Send("BattleMainUI", "GetPlayerHp");
        BattleCore.I.ShowBloodScreen();
    }
    #region ==== 🎨 ORDERING IN LAYER ====
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    #endregion
}
