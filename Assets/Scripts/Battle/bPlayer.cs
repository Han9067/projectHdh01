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
        InitParts();
    }
    void Start()
    {
        setAppearance();
    }
    void InitParts()
    {
        foreach (PtType PtType in System.Enum.GetValues(typeof(PtType)))
        {
            string partName = PtType.ToString();
            ptSpr[PtType] = ptMain.transform.Find(partName).GetComponent<SpriteRenderer>();
        }
    }
    void setAppearance()
    {
        var pData = PlayerManager.I.pData;
        Color skinColor = CharColor.GetSkinColor(pData.Skin);
        Color hairColor = CharColor.GetHairColor(pData.HairColor);
        PtType[] skinParts = new PtType[] { PtType.Face, PtType.Ear, PtType.BaseBody, PtType.BaseHand1A, PtType.BaseHand1A2, PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth };
        foreach (PtType skinPart in skinParts)
            ptSpr[skinPart].color = skinColor;
        PtType[] hairParts = new PtType[] { PtType.FrontHair1, PtType.FrontHair2, PtType.BackHair };
        foreach (PtType hairPart in hairParts)
            ptSpr[hairPart].color = hairColor;
        switch (pData.Hair)
        {
            case 1:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(false);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                ptSpr[PtType.FrontHair1].color = hairColor;
                ptSpr[PtType.BackHair].color = hairColor;
                break;
            case 2:
            case 3:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(false);
                ptSpr[PtType.BackHair].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                ptSpr[PtType.FrontHair1].color = hairColor;
                break;
            case 100:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(true);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                ptSpr[PtType.FrontHair2].color = hairColor;
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                ptSpr[PtType.BackHair].color = hairColor;
                break;
        }

        PtType[] bParts = new PtType[] {PtType.BaseHand1A, PtType.BaseHand1A2, PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth,
            PtType.EqBody, PtType.EqHand1A, PtType.EqHand1B, PtType.EqHand2, PtType.EqBoth, PtType.OneWp1, PtType.OneWp2, PtType.TwoWp1, PtType.TwoWp2, PtType.TwoWp3 };
        foreach (PtType bbb in bParts)
            ptSpr[bbb].gameObject.SetActive(false);

        int[] handState = new int[2] { -1, -1 };
        string[] handKeys = new string[] { "Hand1", "Hand2" };
        for (int i = 0; i < 2; i++)
        {
            if (pData.EqSlot[handKeys[i]] != null)
            {
                handState[i] = pData.EqSlot[handKeys[i]].Both;
                SetHandSprite(pData.EqSlot[handKeys[i]], handState[i]);
            }
        }

        if (pData.EqSlot["Armor"] != null)
        {
            string eqStr = pData.EqSlot["Armor"].ItemId.ToString();
            ptSpr[PtType.EqBody].sprite = ResManager.GetSprite(eqStr + "_body");
            ptSpr[PtType.EqHand1A].sprite = ResManager.GetSprite(eqStr + "_hand1A");
            ptSpr[PtType.EqHand1B].sprite = ResManager.GetSprite(eqStr + "_hand1B");
            ptSpr[PtType.EqHand2].sprite = ResManager.GetSprite(eqStr + "_hand2");
            ptSpr[PtType.EqBoth].sprite = ResManager.GetSprite(eqStr + "_both");

            ptSpr[PtType.EqBody].gameObject.SetActive(true);
            switch (handState[0])
            {
                case -1: ptSpr[PtType.BaseHand1A].gameObject.SetActive(true); ptSpr[PtType.EqHand1A].gameObject.SetActive(true); break;
                case 0: ptSpr[PtType.BaseHand1B].gameObject.SetActive(true); ptSpr[PtType.EqHand1B].gameObject.SetActive(true); break;
                case 1: ptSpr[PtType.BaseBoth].gameObject.SetActive(true); ptSpr[PtType.EqBoth].gameObject.SetActive(true); break;
                case 2: ptSpr[PtType.BaseHand1A].gameObject.SetActive(true); ptSpr[PtType.BaseHand1A2].gameObject.SetActive(true); ptSpr[PtType.EqHand1A].gameObject.SetActive(true); break;
            }
            if (handState[1] != 1)
            {
                ptSpr[PtType.BaseHand2].gameObject.SetActive(true);
                ptSpr[PtType.EqHand2].gameObject.SetActive(true);
            }
        }
    }
    private void SetHandSprite(ItemData data, int handIndex)
    {
        var sprType = data.Both switch
        {
            0 => handIndex == 0 ? PtType.OneWp1 : PtType.OneWp2,
            1 => (handIndex == 0 && data.Type == 2) ? PtType.TwoWp1 : PtType.TwoWp2,
            _ => PtType.TwoWp3
        };

        ptSpr[sprType].gameObject.SetActive(true);
        ptSpr[sprType].sprite = ResManager.GetSprite("wp" + data.ItemId.ToString());
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
