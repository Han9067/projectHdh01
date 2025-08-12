using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
public class bMonster : MonoBehaviour
{
    public int objId;
    public int monsterId;
    public MonData monData;
    public GameObject shdObj, bodyObj;
    public int x, y;
    void Start()
    {
        monData = MonManager.I.MonDataList[monsterId];
        bodyObj.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("mon_" + monsterId);
        shdObj.transform.localScale = new Vector3(monData.SdwScr, monData.SdwScr, 1);
        shdObj.transform.localPosition = new Vector3(0, monData.SdwY, 0);
        bodyObj.transform.localPosition = new Vector3(0, monData.OffY, 0);
    }
    public void SetMonData(int objId, int monId, int x, int y, float px, float py)
    {
        this.objId = objId;
        this.monsterId = monId;
        this.x = x;
        this.y = y;
        transform.position = new Vector3(px, py, 0);
        SetObjLayer(y);
    }
    public void SetObjLayer(int y)
    {
        int ly = y * 100;
        shdObj.GetComponent<SpriteRenderer>().sortingOrder = ly;
        bodyObj.GetComponent<SpriteRenderer>().sortingOrder = ly + 1;
    }
}
