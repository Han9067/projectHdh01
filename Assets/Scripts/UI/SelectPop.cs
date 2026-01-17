using GB;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectPop : UIScreen
{
    [SerializeField] private GameObject pop;
    public List<GameObject> selList = new List<GameObject>();
    private ItemData selItem;

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
                int hei = 1;
                //순차적으로 0 :구매, 1: 판매, 2: 정보, 3: 사용하기, 4: 장착하기
                for (int i = 0; i < selList.Count; i++)
                    selList[i].SetActive(false);
                int n = data.Get<int>();
                switch (n)
                {
                    case 0: //구매
                    case 1: //판매
                    case 2:
                        //전투 화면에서 몬스터, NPC 확인?
                        selList[n].SetActive(true);
                        break;
                    case 3:
                        //무기, 장비 아이템
                        selList[4].SetActive(true);
                        selList[5].SetActive(true);
                        hei = 2;
                        break;
                    case 4:
                        //해당 아이템이 내부 인벤에 있으며 소모형 아이템일 경우
                        selList[3].SetActive(true);
                        selList[5].SetActive(true);
                        hei = 2;
                        break;
                    case 5:
                        //기타 아이템
                        selList[5].SetActive(true); //버리기
                        break;
                    case 101:
                        //월드맵 -> 숲지역에서 사냥
                        selList[6].SetActive(true);
                        // selList[7].SetActive(true);
                        selList[8].SetActive(true);
                        hei = 2;
                        break;
                }
                RectTransform popRect = pop.GetComponent<RectTransform>();
                if (popRect != null)
                {
                    Vector2 newSize = popRect.sizeDelta;
                    newSize.y = (hei * 40) + 60;
                    popRect.sizeDelta = newSize;
                }
                break;
            case "SetItemData":
                selItem = data.Get<ItemData>();
                break;
            case "OnBuy":
                if (PlayerManager.I.pData.Crown < selItem.Price)
                {
                    Debug.Log("돈이 부족합니다.");
                    return;
                }
                PlayerManager.I.pData.Crown -= selItem.Price;
                Presenter.Send("WorldMainUI", "UpdateCrownTxt");
                Presenter.Send("InvenPop", "AddItem", selItem);
                Presenter.Send("CityEnterPop", "AddNpcRls", 2); //호감도
                Close();
                break;
            case "OnSell":
                PlayerManager.I.pData.Crown += (int)(selItem.Price * 0.6);
                Presenter.Send("WorldMainUI", "UpdateCrownTxt");
                Presenter.Send("InvenPop", "DeleteItem", selItem.Uid);
                Presenter.Send("CityEnterPop", "AddNpcRls", 2); //호감도
                Close();
                break;
            case "OnInfo":
                //전투 상황에서 오브젝트의 정보
                BattleCore.I.ShowObjInfo();
                Close();
                break;
            case "OnUse":
                foreach (var v in selItem.Att)
                {
                    switch (v.Key)
                    {
                        case 101: PlayerManager.I.AddSkExp(9, v.Value); break;
                        case 102: PlayerManager.I.AddSkExp(10, v.Value); break;
                        case 103: PlayerManager.I.AddSkExp(11, v.Value); break;
                        case 104: PlayerManager.I.AddSkExp(12, v.Value); break;
                        case 105: PlayerManager.I.AddSkExp(13, v.Value); break;
                        case 106: PlayerManager.I.AddSkExp(14, v.Value); break;
                        case 107: PlayerManager.I.AddSkExp(15, v.Value); break;
                        case 201:
                            PlayerManager.I.pData.HP += v.Value;
                            if (PlayerManager.I.pData.HP > PlayerManager.I.pData.MaxHP)
                                PlayerManager.I.pData.HP = PlayerManager.I.pData.MaxHP;
                            UpdateStateUI();
                            break;
                        case 202:
                            PlayerManager.I.pData.MP += v.Value;
                            if (PlayerManager.I.pData.MP > PlayerManager.I.pData.MaxMP)
                                PlayerManager.I.pData.MP = PlayerManager.I.pData.MaxMP;
                            UpdateStateUI();
                            break;
                        case 203:
                            PlayerManager.I.pData.SP += v.Value;
                            if (PlayerManager.I.pData.SP > PlayerManager.I.pData.MaxSP)
                                PlayerManager.I.pData.SP = PlayerManager.I.pData.MaxSP;
                            UpdateStateUI();
                            break;
                    }
                }
                Presenter.Send("InvenPop", "DeleteItem", selItem.Uid);
                Close();
                break;
            case "OnEq":
                break;
            case "OnDrop":
                break;
            case "OnMove":
                WorldCore.I.SetMovePlayer();
                Close();
                break;
            case "OnSearch":
                Close();
                break;
            case "OnHunt":
                WorldCore.I.SetMovePlayer();
                WorldCore.worldWorkId = 101;
                Close();
                break;
            case "OnQuit":
                Close();
                break;
        }
    }
    private void UpdateStateUI()
    {
        switch (GsManager.gameState)
        {
            case GameState.World:
                Presenter.Send("WorldMainUI", "UpdateInfo");
                break;
            case GameState.Battle:
                Presenter.Send("BattleMainUI", "UpdateInfo");
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