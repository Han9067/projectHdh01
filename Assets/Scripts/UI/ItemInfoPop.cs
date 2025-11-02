using GB;
using System.Collections;
using UnityEngine;
using System.Linq;

public class ItemInfoPop : UIScreen
{
    [SerializeField] private GameObject pop;
    public static bool isActive = false;
    private RectTransform statRect, popRect;
    private void Awake()
    {
        Regist();
        RegistButton();
        pop.transform.position = new Vector3(0, 2000, 0);
        statRect = mTexts["Stat"].GetComponent<RectTransform>();
        popRect = pop.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Presenter.Bind("ItemInfoPop", this);
        pop.transform.position = new Vector3(0, 2000, 0);
        isActive = true;
    }

    private void OnDisable()
    {
        Presenter.UnBind("ItemInfoPop", this);
        pop.transform.position = new Vector3(0, 2000, 0);
        isActive = false;
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });

    }

    public void OnButtonClick(string key)
    {
        // switch(key)
        // {
        // }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "OnItemInfo":
                ItemData itemData = data.Get<ItemData>();
                mTexts["Name"].text = itemData.Name;
                mTexts["Grade"].text = GetGrade(itemData.Grade);
                mTexts["Grade"].color = ColorData.GetItemGradeColor(itemData.Grade);
                //440
                string stat = "";
                float tot = 450f;
                float h1 = 40f;
                foreach (var v in itemData.Stat)
                {
                    stat += $"{LocalizationManager.GetValue(GsManager.I.StatDataList[v.Key].Name)}: {v.Value}\n";
                    h1 += 30;
                }
                if (itemData.ItemId < 60000)
                {
                    stat += $"\n{LocalizationManager.GetValue("Durability")} : {itemData.Dur}/{itemData.MaxDur}";
                    h1 += 30;
                }
                mTexts["Stat"].text = stat;
                statRect.sizeDelta = new Vector2(350, h1);
                // int cnt = desc.Count(c => c == '\n');
                mTexts["Desc"].text = LocalizationManager.GetValue($"{itemData.Res}_Desc");
                popRect.sizeDelta = new Vector2(400, tot);
                break;
            case "OnItemPos":
                Vector3 pos = data.Get<Vector3>();
                pop.transform.position = pos;
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