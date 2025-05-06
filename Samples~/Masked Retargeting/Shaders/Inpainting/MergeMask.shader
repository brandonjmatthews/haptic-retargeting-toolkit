Shader "VSTAR/Merge"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _MaskTex ("Texture", 2D) = "white" {}
        _MaskColor ("Mask Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert ( appdata_img v )
            {
                v2f o;
            
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _MaskTex;
            fixed4 _MaskColor;

            fixed4 frag (v2f i) : SV_Target
            {
                // Currently assuming same resolution and perfectly aligned
                // Get Texture Color
                fixed4 texCol = tex2D(_MainTex, i.uv);
                // Get Mask Color, black indicates "dont render"
                fixed4 maskCol = tex2D(_MaskTex, i.uv);
                // Ensure alpha is zero for black

                return texCol * (1.0 - maskCol.a);
            }
            ENDCG
        }
    }
}
