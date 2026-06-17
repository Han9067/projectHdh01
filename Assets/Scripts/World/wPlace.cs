using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wPlace : MonoBehaviour
{
    public int pcId;
    [SerializeField] private SpriteRenderer spr;
    static readonly int shdColorID = Shader.PropertyToID("_ShdColor");
    static readonly int shdAmountID = Shader.PropertyToID("_ShdAmount");
    private MaterialPropertyBlock placeProp;
    void Awake()
    {
        placeProp = new MaterialPropertyBlock();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerManager.I.curPlace = pcId;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerManager.I.curPlace = 0;
    }
    private void OnMouseEnter()
    {
        WorldCore.intoPlace = pcId;
        if (CityEnterPop.isActive || EventPop.isActive || WorldMainUI.isExplore) return;
        StateHighlight(true);
    }
    private void OnMouseExit()
    {
        WorldCore.intoPlace = 0;
        if (CityEnterPop.isActive || EventPop.isActive || WorldMainUI.isExplore) return;
        StateHighlight(false);
    }
    public void StateHighlight(bool on)
    {
        spr.GetPropertyBlock(placeProp);
        placeProp.SetColor(shdColorID, on ? Color.white : Color.clear);
        placeProp.SetFloat(shdAmountID, on ? 0.35f : 0f);
        spr.SetPropertyBlock(placeProp);
    }
}
