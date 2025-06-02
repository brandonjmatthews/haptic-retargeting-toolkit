//
// HRTK: Output.compute
//
// Copyright (c) 2023 Brandon Matthews
//

Shader "HRTK/MaskedRetargeting/Output" {
	Properties
	{
        [HideInInspector] _LeftTex ("Texture", 2D) = "white" {}
        [HideInInspector] _RightTex ("Texture", 2D) = "white" {}
	}
	SubShader{
        Tags { "RenderType"="Opaque" }

        Cull Off
        Zwrite Off
        Blend One Zero

        Pass{
        CGPROGRAM
        #include "UnityCG.cginc"

        #pragma target 3.0

        #pragma vertex vert
        #pragma fragment frag

        sampler2D _LeftTex;
        sampler2D _RightTex;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
            // float2 screenPos : TEXCOORD1;
            int stereoEyeIndex : TEXCOORD2;
            // UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata v)
        {
            v2f o;

            // UNITY_SETUP_INSTANCE_ID(v);
            // UNITY_INITIALIZE_OUTPUT(v2f, o);
            // UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.vertex = UnityObjectToClipPos(v.vertex);
            // o.screenPos = ComputeScreenPos(o.vertex);
            o.uv = v.uv;
            o.stereoEyeIndex = unity_StereoEyeIndex;
            return o;
        }

        float4 frag(v2f i) : COLOR {
            // UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            fixed4 leftCol = tex2D(_LeftTex, i.uv);
            fixed4 rightCol = tex2D(_RightTex, i.uv);
            return float4(i.stereoEyeIndex == 0 ? leftCol : rightCol);
        }

        ENDCG
        }
	}
	Fallback off
}