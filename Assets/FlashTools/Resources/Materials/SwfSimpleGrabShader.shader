Shader "FlashTools/SwfGrabSimple" {
	Properties {
		[PerRendererData] _MainTex ("Main Texture", 2D   ) = "white" {}
		[PerRendererData] _Tint    ("Tint"        , Color) = (1,1,1,1)

		[Enum(UnityEngine.Rendering.BlendOp  )] _BlendOp  ("BlendOp" , Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Int) = 10
	}

	SubShader {
		Tags {
			"Queue"             = "Transparent"
			"IgnoreProjector"   = "True"
			"RenderType"        = "Transparent"
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull     Off
		Lighting Off
		ZWrite   Off

		BlendOp [_BlendOp]
		Blend [_SrcBlend] [_DstBlend]

		GrabPass { }

		Pass {
		CGPROGRAM
			fixed4    _Tint;
			sampler2D _MainTex;
			sampler2D _GrabTexture;
			sampler2D _AlphaTex;
			float     _AlphaSplitEnabled;

			#pragma multi_compile FT_DARKEN_BLEND FT_DIFFERENCE_BLEND FT_INVERT_BLEND FT_OVERLAY_BLEND FT_HARDLIGHT_BLEND

			#include "UnityCG.cginc"
			#include "SwfBaseCG.cginc"

			#pragma vertex swf_grab_vert
			#pragma fragment swf_grab_frag
		ENDCG
		}
	}
}