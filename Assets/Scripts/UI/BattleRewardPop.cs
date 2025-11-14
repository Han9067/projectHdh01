using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;


public class BattleRewardPop : UIScreen
{
    private Transform itemParent;
    private List<List<InvenGrid>> rGrids; // 보상 그리드
    private GridObj[,] gridObjs;
    private void Awake()
    {
        Regist();
        RegistButton();
        InitGrid();
        itemParent = mGameObject["Item"].transform;
    }
    private void InitGrid()
    {
        Transform gridParent = mGameObject["Grid"].transform;
        gridObjs = new GridObj[10, 10];
        rGrids = new List<List<InvenGrid>>();
        for (int y = 0; y < 10; y++)
        {
            List<InvenGrid> row = new List<InvenGrid>();
            for (int x = 0; x < 10; x++)
            {
                string gridName = $"grid_{y}_{x}";
                gridObjs[y, x] = gridParent.Find(gridName).GetComponent<GridObj>();
                gridObjs[y, x].SetGrid(y, x);
                row.Add(new InvenGrid { x = x, y = y, slotId = -1 });
            }
            rGrids.Add(row);
        }
    }
    private void OnEnable()
    {
        Presenter.Bind("BattleRewardPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("BattleRewardPop", this);
        //해당 팝업 아이템 전부 삭제
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
            case "ClickComp":
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetReward":
                List<int> list = ItemManager.I.RewardItemIdList;
                foreach (var i in list)
                {
                    int id = list[i];
                    var itemData = ItemManager.I.ItemDataList[id].Clone();
                    GameObject obj = Instantiate(ResManager.GetGameObject("ItemObj"), itemParent);
                    obj.name = $"ItemObj_{itemData.ItemId}";
                    obj.GetComponent<ItemObj>().SetItemData(itemData, 2);
                    //
                }
                ItemManager.I.RewardItemIdList.Clear();
                break;
        }
    }

    public override void Refresh() { }



}