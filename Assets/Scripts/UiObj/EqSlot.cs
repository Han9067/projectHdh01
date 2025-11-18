using UnityEngine;
using UnityEngine.UI;
using GB;

public class EqSlot : MonoBehaviour
{
    public string eq;
    [SerializeField] private Image main;
    [SerializeField] private Image box;
    private Button btn;
    private bool isPossible = false;
    private Color gray = new Color(192 / 255f, 192 / 255f, 192 / 255f, 1), green = new Color(129 / 255f, 183 / 255f, 127 / 255f, 1);
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => { OnButtonClick(); });
    }
    public void StateMain(bool isActive)
    {
        main.gameObject.SetActive(isActive);
    }
    public void StatePossible(bool on)
    {
        isPossible = on;
        if (isPossible)
            box.color = green;
        else
            box.color = gray;
    }
    public void OnButtonClick()
    {
        if (!isPossible) return;
        Presenter.Send("InvenPop", "EquipItem", eq);
    }
}
