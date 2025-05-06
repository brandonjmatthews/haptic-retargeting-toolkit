Shader "Hidden/SimpleMetaball"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0.001,1)) = 0.5
        _Fade ("Fade Distance", Range(0.001, 1)) = 0.02
        _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _MainTex_Size ("Texture Size", Vector) = (0, 0, 0, 0)
        // _OutlineColor ("Outline Color", Color) = (0,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            fixed _Cutoff;
            half4 _Color;
            fixed _Fade;
            // half4 _OutlineColor;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float2 _MainTex_Size;

            float dither(float2 pos, float alpha) {
                pos *= _MainTex_Size.xy;

                // Define a dither threshold matrix which can
                // be used to define how a 4x4 set of pixels
                // will be dithered
                float DITHER_THRESHOLDS[16] =
                {
                    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
                };
                uint modX = uint(pos.x) % 4;
                uint modY = uint(pos.y) % 4;

                int index = (modX * 4) + modY;
                // int index = (int(pos.x) % 4) * 4 + int(pos.y) % 4;
                return alpha - DITHER_THRESHOLDS[index];
            }

            float map(float value, float min1, float max1, float min2, float max2) {
                // Convert the current value to a percentage
                // 0% - min1, 100% - max1
                float perc = (value - min1) / (max1 - min1);

                // Do the same operation backwards with min2 and max2
                value = perc * (max2 - min2) + min2;
                return value;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float avg = (col.r + col.b + col.g) / 3.0;
                float mapped = map(avg, _Cutoff, _Fade, 0.0, 1.0);
                col = lerp(fixed4(0,0,0,0), _Color, step(0, dither(i.uv, mapped)));
               
                return col;
            }
            ENDCG
        }
    }
}
