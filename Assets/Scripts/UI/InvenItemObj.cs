using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GB;

public class InvenItemObj : MonoBehaviour, IPointerClickHandler
{
    public ItemData itemData;
    public int x, y, uid;
    public string eq = "";
    public void SetItemData(ItemData data, int xx, int yy)
    {
        itemData = data;
        GetComponent<Image>().sprite = ResManager.GetSprite(itemData.Res);
        x = xx; y = yy; uid = data.Uid;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        SendItemObj();
    }
    public void SendItemObj()
    {
        string str = $"{itemData.Uid}_{itemData.ItemId}_{itemData.Type}";
        Presenter.Send("InvenPop","ClickObj", str);
    }
}