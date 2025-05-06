Shader "VSTAR/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _Cutoff("Alpha cutoff", Range(0,.9)) = .5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="TransparentCutout"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            fixed4 _OutlineColor;
            fixed _Cutoff;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                clip(col.a - _Cutoff);
                clip(col.rgb - _OutlineColor.rgb);
                return col;
            }
            ENDCG
        }
    }
}
