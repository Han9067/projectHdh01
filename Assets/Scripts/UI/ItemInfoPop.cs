using GB;
using System.Collections;
using UnityEngine;

public class ItemInfoPop : UIScreen
{
    [SerializeField] private GameObject pop;
    public static bool isActive = false;
    private void Awake()
    {
        Regist();
        RegistButton();
        pop.transform.position = new Vector3(0, 2000, 0);
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
                // string stat = "";
                if (itemData.ItemId > 60000)
                {
                    //아이템
                }
                else if (itemData.ItemId > 30000)
                {
                    //무기
                }
                else
                {
                    //장비
                }
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