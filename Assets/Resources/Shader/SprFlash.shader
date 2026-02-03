Shader "Custom/SprFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        
        // --- 피격 효과 속성 ---
        _HitColor ("Hit Color", Color) = (1,1,1,1)     // 기본 흰색
        _HitAmount ("Hit Amount", Range(0, 1)) = 0     // 0:평소, 1:피격
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            fixed4 _Color;
            fixed4 _HitColor;
            float _HitAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 텍스처 색상 가져오기
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 2. 피격 효과 (Lerp)
                // 원래 색(c.rgb)과 피격 색(_HitColor.rgb)을 _HitAmount 비율로 섞음
                c.rgb = lerp(c.rgb, _HitColor.rgb, _HitAmount);
                
                // 3. 투명도 적용
                c.rgb *= c.a;
                
                return c;
            }
        ENDCG
        }
    }
}