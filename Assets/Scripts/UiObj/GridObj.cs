using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int y, x;
    private Image img;
    void Awake()
    {
        img = GetComponent<Image>();
    }
    public void SetGrid(int y, int x)
    {
        this.y = y;
        this.x = x;
    }
    public void ChangeColor(Color color)
    {
        img.color = color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 버튼 위에 올라왔을 때 한 번 실행
        // Debug.Log(y + ", " + x);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 버튼 밖으로 나갔을 때 한 번 실행
    }
}
