//
// HRTK: DepthMaskTexture.shader
//
// Copyright (c) 2023 Brandon Matthews
//


Shader "Transparent/DepthMaskTexture" {
	Properties

	{
		_MainTex("Base Mask", 2D) = "white" {}
		_Cutoff("Base Alpha cutoff", Range(0,.9)) = .5
	}

	SubShader{

		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Geometry-10" }

		// Don't draw in the RGBA channels; just the depth buffer

		ColorMask 0
		ZWrite On

		Pass {
			AlphaTest LEqual[_Cutoff]
			SetTexture[_MainTex] {
				combine texture * primary, texture
			}
		}
	}
}
