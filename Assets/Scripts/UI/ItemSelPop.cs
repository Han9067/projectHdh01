using UnityEngine;
using GB;
using Unity.VisualScripting;


public class ItemSelPop : UIScreen
{
    public GameObject mainObj;
    public GameObject popObj;
    private ItemData selItem;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("ItemSelPop", this);
        selItem = null;
    }
    private void OnDisable()
    {
        Presenter.UnBind("ItemSelPop", this);

    }
    public void RegistButton()
    {
        // Debug.Log("RegistButton 호출");
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "ClickItemSelPopPannel":
            case "ClickItemBack":
                Close();
                break;
            case "ClickItemBuy":
                if (PlayerManager.I.pData.Crown < selItem.Price)
                {
                    Debug.Log("돈이 부족합니다.");
                    return;
                }
                Vector2 pos = PlayerManager.I.CanAddItem(selItem.W, selItem.H);
                if (pos.x == -1 || pos.y == -1)
                {
                    Debug.Log("인벤토리가 꽉 찼습니다.");
                    return;
                }
                PlayerManager.I.pData.Crown -= selItem.Price;
                Presenter.Send("WorldMainUI", "UpdateGoldTxt", PlayerManager.I.pData.Crown.ToString());
                ItemManager.I.CreateInvenItem(selItem.ItemId, (int)pos.x, (int)pos.y);
                Presenter.Send("InvenPop", "AddItem", PlayerManager.I.pData.Inven[PlayerManager.I.pData.Inven.Count - 1]);
                break;
            case "ClickItemSell":
                // 판매 로직
                UnityEngine.Debug.Log("ItemSell");
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        string[] btnArr = { "ClickItemBuy", "ClickItemSell", "ClickItemThrow", "ClickItemBack" };
        for (int i = 0; i < btnArr.Length; i++)
            mButtons[btnArr[i]].gameObject.SetActive(false);
        string[] objArr = { "NameBox", "GoldBox" };
        for (int i = 0; i < objArr.Length; i++)
            mGameObject[objArr[i]].SetActive(false);
        switch (key)
        {
            case "SendItemData":
                selItem = data.Get<ItemData>();
                break;
            case "StateItemBuy":
                string[] strArr = data.Get<string>().Split('-');
                float x = float.Parse(strArr[0]);
                float y = float.Parse(strArr[1]);
                mainObj.transform.position = new Vector3(x, y, 0);
                mButtons["ClickItemBuy"].gameObject.SetActive(true);
                mButtons["ClickItemBack"].gameObject.SetActive(true);
                float cx = mainObj.transform.position.x, cy = mainObj.transform.position.y;
                mButtons["ClickItemBuy"].gameObject.transform.position = new Vector3(cx, cy - 50, 0);
                mButtons["ClickItemBack"].gameObject.transform.position = new Vector3(cx, cy - 130, 0);
                mGameObject["NameBox"].SetActive(true);
                mGameObject["GoldBox"].SetActive(true);
                mGameObject["NameBox"].gameObject.transform.position = new Vector3(cx, cy + 165, 0);
                mGameObject["GoldBox"].gameObject.transform.position = new Vector3(cx, cy + 50, 0);
                mTexts["NameBoxText"].text = selItem.Name;
                mTexts["GoldBoxText"].text = selItem.Price.ToString();
                break;
        }
    }

    public override void Refresh()
    {

    }
}