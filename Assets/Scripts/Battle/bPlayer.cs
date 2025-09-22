using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEditor;

public class bPlayer : MonoBehaviour
{
    public int objId = 1000;
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain;
    void Awake()
    {
        HumanAppearance.I.InitParts(ptSpr, ptMain);
    }
    void Start()
    {
        HumanAppearance.I.SetObjAppearance(0, ptSpr);
    }
    public void OnDamaged(int dmg)
    {
        //플레이어 피격!
        PlayerManager.I.pData.HP -= dmg;
        if (PlayerManager.I.pData.HP <= 0)
        {
            PlayerManager.I.pData.HP = 0;
            Debug.Log("Player Dead");
        }
        Presenter.Send("BattleMainUI", "GetPlayerHp");

        BattleCore.I.ShowDmgTxt(dmg, transform.position); // 데미지 텍스트 표시
    }
    #region ==== 🎨 ORDERING IN LAYER ====
    public void SetObjLayer(int y)
    {
        int layer = y * 100;

        int childCount = ptMain.transform.childCount;
        PtType[] layerOrder = new PtType[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = ptMain.transform.GetChild(i);
            string childName = child.name;

            // 자식 오브젝트 이름을 PtType으로 파싱
            if (System.Enum.TryParse<PtType>(childName, out PtType ptType))
                layerOrder[i] = ptType;
        }

        // 순서대로 레이어 설정
        for (int i = 0; i < layerOrder.Length; i++)
        {
            if (ptSpr.ContainsKey(layerOrder[i]))
                ptSpr[layerOrder[i]].sortingOrder = layer + i;
        }
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
            myScript.OnDamaged(2);
        }
    }
}
