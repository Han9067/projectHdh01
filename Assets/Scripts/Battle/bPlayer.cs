using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEditor;
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
    public void OnDamaged(int att, int crt, int crtRate)
    {
        // Debug.Log("OnDamaged: " + att + " " + crt + " " + crtRate);
        int dmg = GsManager.I.GetDamage(att, pData.Def);
        pData.HP -= dmg;
        if (pData.HP <= 0)
        {
            pData.HP = 0;
            Debug.Log("Player Dead");
        }
        Presenter.Send("BattleMainUI", "GetPlayerHp");
        BattleCore.I.ShowBloodScreen();
        BattleCore.I.ShowDmgTxt(dmg, transform.position); // 데미지 텍스트 표시
    }
    #region ==== 🎨 ORDERING IN LAYER ====
    public void SetObjLayer(int y)
    {
        sGrp.sortingOrder = y;
    }
    #endregion
}

[CustomEditor(typeof(bPlayer))]
public class bPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        bPlayer myScript = (bPlayer)target;

        if (GUILayout.Button("체력 차감"))
        {
            myScript.OnDamaged(2, 0, 0);
        }
    }
}
