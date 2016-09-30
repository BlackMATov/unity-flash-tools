Shader "FlashTools/SwfMasked" {
	Properties {
		[PerRendererData] _MainTex ("Main Texture", 2D   ) = "white" {}
		[PerRendererData] _Tint    ("Tint"        , Color) = (1,1,1,1)

		_StencilID ("Stencil ID", Int) = 0
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

		Pass {
			Stencil {
				Ref  [_StencilID]
				Comp Equal
			}
		CGPROGRAM
			fixed4    _Tint;
			sampler2D _MainTex;
			sampler2D _GrabTexture;

			#include "UnityCG.cginc"
			#include "SwfBaseCG.cginc"

			#pragma vertex swf_vert
			#pragma fragment swf_frag
		ENDCG
		}
	}
}