using UnityEngine;
using TMPro;
using GB;


public class NpcListPop : UIScreen
{
    private int cityId;
    [SerializeField] private Transform listParent;
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
                cityId = data.Get<int>();
                mTMPText["Title"].text = LocalizationManager.GetValue("Square");
                break;
        }
    }

    public override void Refresh()
    {
    }
}