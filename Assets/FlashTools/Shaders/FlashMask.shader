Shader "FlashTools/FlashMask" {
	Properties {
		[PerRendererData] _MainTex   ("Main Texture", 2D ) = "white" {}
		_StencilID ("Stencil ID"  , Int) = 0
	}

	SubShader {
		Tags {
			"Queue"             = "Transparent"
			"IgnoreProjector"   = "True"
			"RenderType"        = "Transparent"
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		ColorMask 0
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass {
			Stencil {
				Ref   [_StencilID]
				Comp always
				Pass replace
			}
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			v2f vert(appdata_t IN) {
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv     = IN.uv;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target {
				fixed4 c = tex2D(_MainTex, IN.uv);
				if ( c.a < 0.01 ) {
					discard;
				}
				return c;
			}
		ENDCG
		}
	}
}