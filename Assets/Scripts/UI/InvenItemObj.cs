using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GB;

public class InvenItemObj : MonoBehaviour, IPointerClickHandler
{
    public ItemData itemData;
    public int x, y, uid, state; //state : 0 인벤토리 존재, 1 인벤토리 존재 안함
    public string eq = "";
    public void SetItemData(ItemData data, int xx, int yy)
    {
        itemData = data;
        GetComponent<Image>().sprite = Resources.Load<Sprite>(itemData.Path);
        x = xx; y = yy; uid = data.Uid;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        SendItemObj();
    }
    public void SendItemObj()
    {
        Debug.Log(uid);
        if(eq != "") PlayerManager.I.TakeoffEq(eq);
        string str = $"{itemData.Uid}_{itemData.ItemId}_{itemData.Type}";
        Presenter.Send("InvenPop","ClickObj", str);
    }
}