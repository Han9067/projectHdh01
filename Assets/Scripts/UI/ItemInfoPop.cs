using GB;
using System.Collections;
using UnityEngine;
using System.Linq;

public class ItemInfoPop : UIScreen
{
    [SerializeField] private GameObject pop;
    public static bool isActive = false;
    private RectTransform statRect, popRect;
    private int iType = 0;
    private void Awake()
    {
        Regist();
        pop.transform.position = new Vector3(0, 2000, 0);
        statRect = mTMPText["Stat"].GetComponent<RectTransform>();
        popRect = pop.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Presenter.Bind("ItemInfoPop", this);
        pop.transform.position = new Vector3(0, 2000, 0);
        isActive = true;
        iType = 0;
    }

    private void OnDisable()
    {
        Presenter.UnBind("ItemInfoPop", this);
        pop.transform.position = new Vector3(0, 2000, 0);
        isActive = false;
        iType = 0;
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "OnItemType":
                int type = data.Get<int>();
                iType = type;
                break;
            case "OnItemInfo":
                ItemData itemData = data.Get<ItemData>();
                mTMPText["Name"].text = LocalizationManager.GetValue(itemData.Name);
                mTMPText["Grade"].text = GetGrade(itemData.Grade);
                mTMPText["Grade"].color = ColorData.GetItemGradeColor(itemData.Grade);
                //440
                string stat = "";
                float tot = 380;
                float h1 = 30f;
                foreach (var v in itemData.Att)
                {
                    if (v.Key == 0)
                        stat = LocalizationManager.GetValue("None");
                    else
                        stat += $"{LocalizationManager.GetValue(GsManager.I.AttDataList[v.Key].Name)}: {v.Value}\n";
                    h1 += 30;
                }
                if (itemData.ItemId < 60000)
                {
                    if (itemData.Type != 12)
                        stat += $"\n{LocalizationManager.GetValue("Durability")} : {itemData.Dur}/{itemData.MaxDur}";
                    else
                        stat += $"\n{LocalizationManager.GetValue("Quantity")} : {itemData.Dur}/{itemData.MaxDur}";
                    h1 += 30;
                }
                tot += h1;
                mTMPText["Stat"].text = stat;
                statRect.sizeDelta = new Vector2(350, h1);
                // int cnt = desc.Count(c => c == '\n');
                mTMPText["Desc"].text = LocalizationManager.GetValue($"{itemData.Res}_Desc");
                popRect.sizeDelta = new Vector2(400, tot);
                mGameObject["Crown"].SetActive(ShopInvenPop.isActive);
                if (ShopInvenPop.isActive)
                {
                    int price = ItemManager.I.GetItemPrice(itemData.Price, itemData.Dur, itemData.MaxDur);
                    if (iType != 10) price = (int)(price * 0.6);
                    mTMPText["CrownVal"].text = price.ToString();
                }
                break;
            case "OnItemPos":
                Vector3 pos = data.Get<Vector3>();
                pop.transform.position = pos;

                float cvsY = popRect.parent.GetComponent<RectTransform>().rect.max.y;
                float halfH = popRect.rect.height * popRect.lossyScale.y * 0.5f;

                Vector2 anch = popRect.anchoredPosition;
                anch.y = Mathf.Min(anch.y, cvsY - halfH);
                popRect.anchoredPosition = anch;
                break;
        }
    }
    private string GetGrade(int grade)
    {
        string g;
        switch (grade)
        {
            case 2: g = "G"; break;
            case 3: g = "F"; break;
            case 4: g = "E"; break;
            case 5: g = "D"; break;
            case 6: g = "C"; break;
            case 7: g = "B"; break;
            case 8: g = "A"; break;
            case 9: g = "S"; break;
            case 10: g = "SS"; break;
            default: g = "H"; break;
        }
        return string.Format(LocalizationManager.GetValue("ItemGrade"), g);
    }
    public override void Refresh()
    {

    }
}