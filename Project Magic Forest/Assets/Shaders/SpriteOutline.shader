Shader "Magic Forest/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float outlineSteps = 8.0;
                float maxAlpha = 0.0;

                // Sample alpha in a circle around the current pixel
                for (float angle = 0.0; angle < 6.28318; angle += 6.28318 / outlineSteps)
                {
                    float2 offset = float2(cos(angle), sin(angle)) * _OutlineWidth * _MainTex_TexelSize.xy * 100.0;
                    float alpha = tex2D(_MainTex, uv + offset).a;
                    maxAlpha = max(maxAlpha, alpha);
                }

                fixed4 col = tex2D(_MainTex, uv);
                
                // If there's alpha around us but not at this pixel, draw outline
                if (col.a < 0.5 && maxAlpha > 0.5)
                {
                    col = _OutlineColor;
                    col.a = maxAlpha;
                }
                
                // Apply vertex color
                col *= i.color;
                return col;
            }
            ENDCG
        }
    }
}
