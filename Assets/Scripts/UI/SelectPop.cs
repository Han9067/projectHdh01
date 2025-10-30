using GB;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectPop : UIScreen
{
    [SerializeField] private GameObject pop;
    public List<GameObject> selList = new List<GameObject>();
    #region 아이템
    private ItemData selItem;
    #endregion

    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("SelectPop", this);
        StartCoroutine(SetPos());
    }

    private void OnDisable()
    {
        Presenter.UnBind("SelectPop", this);
        pop.transform.position = new Vector3(0, 2000, 0);
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "ClickBlock":
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetList":
                //순차적으로 0 :구매, 1: 판매, 2: 정보, 3: 버리기, 4: 취소
                for (int i = 0; i < 4; i++)
                    selList[i].SetActive(false);
                switch (data.Get<int>())
                {
                    case 0:
                        selList[0].SetActive(true);
                        break;
                }
                break;
            case "SetItemData":
                selItem = data.Get<ItemData>();
                break;
            case "OnBuy":
                Debug.Log("OnBuy");
                if (PlayerManager.I.pData.Crown < selItem.Price)
                {
                    Debug.Log("돈이 부족합니다.");
                    return;
                }
                // PlayerManager.I.pData.Crown -= selItem.Price;
                // Presenter.Send("WorldMainUI", "UpdateGoldTxt", PlayerManager.I.pData.Crown.ToString());
                // ItemManager.I.CreateInvenItem(selItem.ItemId, (int)pos.x, (int)pos.y);
                // Presenter.Send("InvenPop", "AddItem", PlayerManager.I.pData.Inven[PlayerManager.I.pData.Inven.Count - 1]);
                break;
            case "OnSell":
                break;
            case "OnInfo":
                break;
            case "OnDrop":
                break;
            case "OnQuit":
                Close();
                break;
        }
    }

    public override void Refresh() { }

    private IEnumerator SetPos()
    {
        yield return new WaitForEndOfFrame(); // 또는 yield return null;
        pop.transform.position = Input.mousePosition;
    }
}