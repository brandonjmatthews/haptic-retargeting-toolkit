//
// HRTK: MaskedUpscale.compute
//
// Copyright (c) 2023 Brandon Matthews
//

Shader "VSTAR/InpaintUpscale"
{
    Properties
    {
        // Source texture
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        // Fill texture
        [HideInInspector] _FillTex ("Texture", 2D) = "white" {}
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

            float EPSILON = 1e-6f;

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            sampler2D _FillTex;

            float4 blerp (float4 c00, float4 c10, float4 c01, float4 c11, float tx, float ty) {
                return lerp(lerp(c00, c10, tx), lerp(c01, c11, tx), ty);
            }
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 destUV : TEXCOORD0;
            };			

            v2f vert ( appdata_img v )
            {
                v2f o;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.destUV = v.texcoord;
                return o; 
            }					
		
            fixed4 frag ( v2f i ) : SV_Target
            {   
                fixed4 fillPixel = tex2D(_FillTex, i.destUV);

                if (fillPixel.r < 0.001) {
                    // Mask pixel
                    float2 srcUV = i.destUV;

                    // Get nearby four pixels
                    float2 uv00 = UnityStereoScreenSpaceUVAdjust(srcUV, _MainTex_ST);
                    float2 uv10 = UnityStereoScreenSpaceUVAdjust(srcUV + _MainTex_TexelSize.xy * half2(0,1), _MainTex_ST);
                    float2 uv01 = UnityStereoScreenSpaceUVAdjust(srcUV + _MainTex_TexelSize.xy * half2(1,0), _MainTex_ST);
                    float2 uv11 = UnityStereoScreenSpaceUVAdjust(srcUV + _MainTex_TexelSize.xy * half2(1,1), _MainTex_ST);

                    // Get Colours
                    float4 c00 = tex2D(_MainTex, uv00);
                    float4 c10 = tex2D(_MainTex, uv10);
                    float4 c01 = tex2D(_MainTex, uv01);
                    float4 c11 = tex2D(_MainTex, uv11);

                    // fillPixel = c00;
                    // Bilinear interpolation
                    fillPixel = blerp(c00, c10, c01, c11, 0.5, 0.5);
                    // fillPixel = float4(1, 0, 0, 1);
                }

                return fillPixel;
            }

            ENDCG
        }
    }
}
