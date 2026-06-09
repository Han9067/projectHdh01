using UnityEngine;
using UnityEngine.EventSystems;
using GB;

public class wCity : MonoBehaviour
{
    public int id;
    [SerializeField] private SpriteRenderer spr;
    static readonly int shdColorID = Shader.PropertyToID("_ShdColor");
    static readonly int shdAmountID = Shader.PropertyToID("_ShdAmount");
    private MaterialPropertyBlock cityProp;
    private void Awake()
    {
        cityProp = new MaterialPropertyBlock();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerManager.I.curPlace = id;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerManager.I.curPlace = 0;
    }
    private void OnMouseEnter()
    {
        WorldCore.intoPlace = id;
        if (CityEnterPop.isActive) return;
        StateHighlight(true);
    }
    private void OnMouseExit()
    {
        WorldCore.intoPlace = 0;
        if (CityEnterPop.isActive) return;
        StateHighlight(false);
    }
    public void StateHighlight(bool on)
    {
        spr.GetPropertyBlock(cityProp);
        cityProp.SetColor(shdColorID, on ? Color.white : Color.clear);
        cityProp.SetFloat(shdAmountID, on ? 0.35f : 0f);
        spr.SetPropertyBlock(cityProp);
    }
}
