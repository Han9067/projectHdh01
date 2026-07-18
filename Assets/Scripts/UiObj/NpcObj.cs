using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GB;
using System.Collections.Generic;

public class NpcObj : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public NpcData data;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private Image high;
    [SerializeField] private GameObject npcMain;
    public Dictionary<string, GameObject> npcSpr;
    public void SetNpcData(NpcData npcData)
    {
        data = npcData;
        nameTxt.text = data.Name;
        high.gameObject.SetActive(false);

        npcSpr = new Dictionary<string, GameObject>();
        string[] all = new string[] { "Face", "Eyebrow", "Eye1", "Eye2", "Ear", "Nose", "Mouth", "BaseBody",
            "BaseHand1A", "BaseHand2", "Hair1A", "Hair1B", "Hair2" ,"Beard", "EqBody","EqHand1A","EqHand2","EqHelmet"};
        foreach (string v in all)
            npcSpr[v] = npcMain.transform.Find(v).gameObject;
        GsManager.I.SetUiBaseParts(data.NpcId, npcSpr, true);
        GsManager.I.SetUiEqParts(data, npcSpr);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging) OnButtonClick();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        high.gameObject.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        high.gameObject.SetActive(false);
    }
    private void OnButtonClick()
    {
        Debug.Log("OnButtonClick");
        UIManager.ShowPopup("NpcInfoPop");
        Presenter.Send("NpcInfoPop", "SetInfo", data);
    }
    // private Color GetColor(int g)
    // {
    //     switch (g)
    //     {
    //         default:
    //     }
    // }
}
