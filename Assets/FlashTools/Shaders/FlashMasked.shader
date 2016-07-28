Shader "FlashTools/FlashMasked" {
	Properties {
		[PerRendererData] _MainTex   ("Main Texture", 2D ) = "white" {}
		[PerRendererData] _StencilID ("Stencil ID"  , Int) = 0
	}

	SubShader {
		Tags {
			"Queue"             = "Transparent"
			"IgnoreProjector"   = "True"
			"RenderType"        = "Transparent"
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass {
			Stencil {
				Ref  [_StencilID]
				Comp Equal
			}
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t {
				float4 vertex   : POSITION;
				float2 uv       : TEXCOORD0;
				float4 mulcolor : COLOR;
				float4 addcolor : TEXCOORD1;
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				float2 uv       : TEXCOORD0;
				fixed4 mulcolor : COLOR;
				fixed4 addcolor : TEXCOORD1;
			};

			v2f vert(appdata_t IN) {
				v2f OUT;
				OUT.vertex   = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv       = IN.uv;
				OUT.mulcolor = IN.mulcolor;
				OUT.addcolor = IN.addcolor;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float     _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture(float2 uv) {
				fixed4 color = tex2D(_MainTex, uv);
			#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;
			#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target {
				fixed4 c = SampleSpriteTexture(IN.uv);
				if ( c.a > 0.01 ) {
					c = c * IN.mulcolor + IN.addcolor;
				}
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}