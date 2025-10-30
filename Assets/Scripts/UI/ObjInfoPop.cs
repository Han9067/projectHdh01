using GB;
using UnityEngine;


public class ObjInfoPop : UIScreen
{



    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("ObjInfoPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("ObjInfoPop", this);

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
            case "ObjInfoPopClose":
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ObjInfoData":
                SetInfo(data.Get<string>());
                break;
        }
    }

    public override void Refresh()
    {
    }

    private void SetInfo(string str)
    {
        string[] strs = str.Split('_');
        mTexts["ObjName"].text = strs[0];
        mTexts["ObjHpVal"].text = strs[1];
        mTexts["ObjAttVal"].text = strs[2];
        mTexts["ObjDefVal"].text = strs[3];
    }

}