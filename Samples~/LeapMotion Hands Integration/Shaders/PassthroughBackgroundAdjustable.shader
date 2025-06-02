//
// HRTK: PassthroughBackgroundAdjustable.shader
// Modified from Ultraleap's PassthroughBackground.shader:
// Apache License, Version 2.0
// Modifications Copyright (c) 2023 Brandon Matthews
//

Shader "HRTK/LeapMotion/Passthrough Background Adjustable" {
	Properties
	{
		[Toggle] _MirrorImageHorizontally ("MirrorImageHorizontally", Float) = 0
		_DeviceID ("DeviceID", Int) = 0
		_OffsetU ("Offset U", Float) = 0.0
        _OffsetV ("Offset V", Float) = 0.0
		_ScaleFactor ("Scale", Float) = 1.0


	}
	SubShader{
	  Tags {"RenderType" = "Opaque" "Queue" = "Background" "IgnoreProjector" = "True"}

	  Cull Off
	  Zwrite Off
	  Blend One Zero

	  Pass{
	  CGPROGRAM
	  #include "../Resources/LeapCG.cginc"
	  #include "UnityCG.cginc"

	  #pragma target 3.0

	  #pragma vertex vert
	  #pragma fragment frag

	  uniform float _LeapGlobalColorSpaceGamma;
	  float _MirrorImageHorizontally;
	  int _DeviceID;
	  float _OffsetU;
	  float _OffsetV;
	  float _ScaleFactor;

	  struct frag_in {
		float4 position : SV_POSITION;
		float4 screenPos  : TEXCOORD1;
		int stereoEyeIndex : TEXCOORD2;

		UNITY_VERTEX_OUTPUT_STEREO
	  };

	  frag_in vert(appdata_img v) {
		frag_in o;

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_OUTPUT(frag_in, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		o.position = UnityObjectToClipPos(v.vertex);
		if(_MirrorImageHorizontally)
		{
			o.screenPos = LeapGetWarpedAndHorizontallyMirroredScreenPos(o.position);
		}
		else
		{
			o.screenPos = LeapGetWarpedScreenPos(o.position);
		}
	
		float2 offsetUV = float2(_OffsetU, _OffsetV);
		o.screenPos = o.screenPos + float4(offsetUV,0,0);
		o.screenPos.xy = (o.screenPos.xy - float2(0.5,0.5)) * _ScaleFactor + float2(0.5, 0.5);

		// set z as the index for the texture array
		o.screenPos.z = _DeviceID + 0.1;
		
		o.stereoEyeIndex = unity_StereoEyeIndex;

		return o;
	  }

	  float4 frag(frag_in i) : COLOR {

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		return float4(i.stereoEyeIndex == 0 ? LeapGetLeftColor(i.screenPos) : LeapGetRightColor(i.screenPos), 1);
	  }

	  ENDCG
	  }
	}
	Fallback off
}
