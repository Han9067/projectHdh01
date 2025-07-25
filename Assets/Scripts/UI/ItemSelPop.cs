using UnityEngine;
using GB;


public class ItemSelPop : UIScreen
{
    public GameObject mainObj;
    public GameObject popObj;
    private int price;

    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("ItemSelPop",this);
    }
    private void OnDisable() 
    {
        Presenter.UnBind("ItemSelPop", this);
        
    }
    public void RegistButton()
    {
        // Debug.Log("RegistButton 호출");
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
    }

    public void OnButtonClick(string key)
    {
        switch(key)
        {
            case "ItemSelPopClose":
                Close();
                break;
            case "ItemBuy":
                // 구매 로직
                UnityEngine.Debug.Log("ItemBuy");
                break;
            case "ItemSell":
                // 판매 로직
                UnityEngine.Debug.Log("ItemSell");
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        string[] str = {"ItemBuy", "ItemSell", "ItemThrow"};
        for(int i = 0; i < str.Length; i++)
            mButtons[str[i]].gameObject.SetActive(false);
        switch(key)
        {
            case "ItemBuy":
                string[] strArr = data.Get<string>().Split('-'); 
                price = int.Parse(strArr[0]);
                float x = float.Parse(strArr[1]);
                float y = float.Parse(strArr[2]);
                mainObj.transform.position = new Vector3(x, y, 0);
                mButtons["ItemBuy"].gameObject.SetActive(true);
                // popObj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
                break;
        }
    }

    public override void Refresh()
    {
            
    }



}