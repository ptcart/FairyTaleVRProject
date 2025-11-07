Shader "UI/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Blur Size", Range(0, 10)) = 2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        GrabPass
        {
            "_BackgroundTexture"
        }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BackgroundTexture;
            float _Size;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = _Size / _ScreenParams.xy;
                float2 uv = i.uv;

                fixed4 col = fixed4(0,0,0,0);
                col += tex2D(_BackgroundTexture, uv + offset);
                col += tex2D(_BackgroundTexture, uv - offset);
                col += tex2D(_BackgroundTexture, uv + float2(offset.x, -offset.y));
                col += tex2D(_BackgroundTexture, uv + float2(-offset.x, offset.y));
                col += tex2D(_BackgroundTexture, uv);
                col /= 5.0;

                return col;
            }
            ENDCG
        }
    }
}
