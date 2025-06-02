//
// HRTK: DepthMaskCustom.shader
//
// Copyright (c) 2023 Brandon Matthews
//


Shader "VSTAR/DepthMaskCustom" {
	Properties

	{
		_MainTex("Base Mask", 2D) = "white" {}
		_Cutoff("Base Alpha cutoff", Range(0,.9)) = .5
	}

	SubShader{

		Tags {"Queue"="Geometry-10"}
		ZWrite On
		ColorMask 0
		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Cutoff;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex); 
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{
			  	fixed4 col = tex2D(_MainTex, i.texcoord);
                clip(_Cutoff - col.a);
                return col;
			}

			ENDCG
		}
	}
}