//
// HRTK: DepthMaskSimple.shader
//
// Copyright (c) 2023 Brandon Matthews
//

Shader "Transparent/DepthMaskSimple" {
	Properties

	{

	}

	SubShader{

		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Geometry-10" }

		// Don't draw in the RGBA channels; just the depth buffer

		ColorMask 0
		ZWrite On

		Pass {}
	}
}
