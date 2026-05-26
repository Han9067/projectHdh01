using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjShd
{
    static readonly int shdColorID = Shader.PropertyToID("_ShdColor");
    static readonly int shdAmountID = Shader.PropertyToID("_ShdAmount");
    public static void ApplyShd(SpriteRenderer spr, MaterialPropertyBlock prop,
        Color color, float amount = 1f)
    {
        spr.GetPropertyBlock(prop);
        prop.SetColor(shdColorID, color);
        prop.SetFloat(shdAmountID, amount);
        spr.SetPropertyBlock(prop);
    }
}
