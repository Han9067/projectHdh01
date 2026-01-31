using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;

public class MatList : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI nameTxt;
    private int itemId, cnt;
    void Start()
    {
        ItemData data = ItemManager.I.ItemDataList[itemId];
        iconImg.sprite = ResManager.GetSprite(data.Res);
        if (data.W >= data.H)
        {
            // 가로가 더 크거나 같으면, width 64 고정, height는 비율 계산
            float ratio = (float)data.H / data.W;
            iconImg.rectTransform.sizeDelta = new Vector2(64f, 64f * ratio);
        }
        else
        {
            // 세로가 더 크면, height 64 고정, width는 비율 계산
            float ratio = (float)data.W / data.H;
            iconImg.rectTransform.sizeDelta = new Vector2(64f * ratio, 64f);
        }
        nameTxt.text = LocalizationManager.GetValue(data.Name) + " (" + cnt + ")";
    }
    public void SetMatObj(int id, int n)
    {
        itemId = id;
        cnt = n;
    }
}
