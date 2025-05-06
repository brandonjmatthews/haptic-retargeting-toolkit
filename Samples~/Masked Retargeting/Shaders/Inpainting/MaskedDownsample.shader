Shader "VSTAR/MaskedDownsample"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
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

            struct v2f
            {
                float4 pos : SV_POSITION;
                half2 uv00 : TEXCOORD0;
                half2 uv01 : TEXCOORD1;
                half2 uv10 : TEXCOORD2;
                half2 uv11 : TEXCOORD3;
            };			

            v2f vert ( appdata_img v )
            {
                v2f o;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                // Get the four nearby pixel UVs
                o.uv00 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy, _MainTex_ST);
                o.uv01 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy * half2(0, 1), _MainTex_ST);
                o.uv10 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy * half2(1, 0), _MainTex_ST);
                o.uv11 = UnityStereoScreenSpaceUVAdjust(v.texcoord + _MainTex_TexelSize.xy * half2(1, 1), _MainTex_ST);

                return o; 
            }					
		
            fixed4 frag ( v2f i ) : SV_Target
            {				
                float4 srcColor00 = tex2D(_MainTex, i.uv00);
                float4 srcColor01 = tex2D(_MainTex, i.uv01);
                float4 srcColor10 = tex2D(_MainTex, i.uv10);
                float4 srcColor11 = tex2D(_MainTex, i.uv11);

                int count = 0;
                float4 color = float4(0,0,0,0);

                if (srcColor00.a > EPSILON) {
                    color += srcColor00;
                    count++;
                }

                if (srcColor01.a > EPSILON) {
                    color += srcColor01;
                    count++;
                }

                if (srcColor10.a > EPSILON) {
                    color += srcColor10;
                    count++;
                }

                if(srcColor11.a > EPSILON) {
                    color += srcColor11;
                    count++;
                }

                if (count > 0) {
                    color = color / count;
                }

                return fixed4(color);
            }

            ENDCG
        }
    }
}
