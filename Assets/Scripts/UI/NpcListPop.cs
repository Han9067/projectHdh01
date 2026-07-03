using UnityEngine;
using GB;
using System.Collections.Generic;

public class NpcListPop : UIScreen
{
    private int cityId;
    [SerializeField] private Transform listParent;
    private List<NpcData> npcData = new List<NpcData>();
    private List<NpcObj> npcObj = new List<NpcObj>();
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("NpcListPop", this);
    }
    private void OnDisable()
    {
        Presenter.UnBind("NpcListPop", this);

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
            case "Leave":
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetSquare":
                if (cityId == data.Get<int>()) return;
                InitNpcList();
                cityId = data.Get<int>();
                npcData = NpcManager.I.GetCityNpcList(cityId);
                mTMPText["Title"].text = LocalizationManager.GetValue("Square");
                foreach (var npc in npcData)
                {
                    var obj = Instantiate(ResManager.GetGameObject("NpcObj"), listParent).GetComponent<NpcObj>();
                    obj.SetNpcData(npc);
                    npcObj.Add(obj);
                }
                break;
        }
    }
    private void InitNpcList()
    {
        if (npcObj.Count == 0) return;
        foreach (var npc in npcObj)
            Destroy(npc.gameObject);
        npcObj.Clear();
        npcData.Clear();
    }
    public override void Refresh() { }
}