#ifndef SWF_BASE_CG_INCLUDED
#define SWF_BASE_CG_INCLUDED

//
//
//

fixed4 swf_darken(fixed4 a, fixed4 b) {
	fixed4 r = min(a, b);
	r.a = b.a;
	return r;
}

fixed4 swf_difference(fixed4 a, fixed4 b) {
	fixed4 r = abs(a - b);
	r.a = b.a;
	return r;
}

fixed4 swf_invert(fixed4 a, fixed4 b) {
	fixed4 r = 1 - a;
	r.a = b.a;
	return r;
}

fixed4 swf_overlay(fixed4 a, fixed4 b) {
	fixed4 r = a > 0.5 ? 1.0 - 2.0 * (1.0 - a) * (1.0 - b) : 2.0 * a * b;
	r.a = b.a;
	return r;
}

fixed4 swf_hardlight(fixed4 a, fixed4 b) {
	fixed4 r = b > 0.5 ? 1.0 - (1.0 - a) * (1.0 - 2.0 * (b - 0.5)) : a * (2.0 * b);
	r.a = b.a;
	return r;
}

//
//
//

struct swf_appdata_t {
	float4 vertex    : POSITION;
	float2 uv        : TEXCOORD0;
	float4 mulcolor  : COLOR;
	float4 addcolor  : TEXCOORD1;
};

struct swf_v2f_t {
	float4 vertex    : SV_POSITION;
	float2 uv        : TEXCOORD0;
	fixed4 mulcolor  : COLOR;
	fixed4 addcolor  : TEXCOORD1;
};

struct swf_grab_v2f_t {
	float4 vertex    : SV_POSITION;
	float2 uv        : TEXCOORD0;
	fixed4 mulcolor  : COLOR;
	fixed4 addcolor  : TEXCOORD1;
	float4 screenpos : TEXCOORD2;
};

//
//
//

inline fixed4 swf_sample_sprite_texture(sampler2D main_tex, float2 uv) {
	fixed4 c = tex2D(main_tex, uv);
#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
	if ( _AlphaSplitEnabled ) {
		c.a = tex2D(_AlphaTex, uv).r;
	}
#endif
	return c;
}

inline swf_v2f_t swf_vert(swf_appdata_t IN) {
	swf_v2f_t OUT;
	OUT.vertex    = mul(UNITY_MATRIX_MVP, IN.vertex);
	OUT.uv        = IN.uv;
	OUT.mulcolor  = IN.mulcolor * _Tint;
	OUT.addcolor  = IN.addcolor;
	return OUT;
}

inline swf_grab_v2f_t swf_grab_vert(swf_appdata_t IN) {
	swf_grab_v2f_t OUT;
	OUT.vertex    = mul(UNITY_MATRIX_MVP, IN.vertex);
	OUT.uv        = IN.uv;
	OUT.mulcolor  = IN.mulcolor * _Tint;
	OUT.addcolor  = IN.addcolor;
	OUT.screenpos = OUT.vertex;
	return OUT;
}

inline fixed4 swf_frag(swf_v2f_t IN) : SV_Target {
	fixed4 c = swf_sample_sprite_texture(_MainTex, IN.uv);
	if ( c.a > 0.01 ) {
		c = c * IN.mulcolor + IN.addcolor;
	}
	c.rgb *= c.a;
	return c;
}

inline fixed4 swf_grab_frag(swf_grab_v2f_t IN) : SV_Target {
	fixed4 c = swf_sample_sprite_texture(_MainTex, IN.uv);
	if ( c.a > 0.01 ) {
		c = c * IN.mulcolor + IN.addcolor;
	}

	float2 grabTexcoord = IN.screenpos.xy / IN.screenpos.w;
	grabTexcoord.x = (grabTexcoord.x + 1.0) * .5;
	grabTexcoord.y = (grabTexcoord.y + 1.0) * .5;
	fixed4 grabColor = tex2D(_GrabTexture, grabTexcoord);

	#if FT_DARKEN_BLEND
		c = swf_darken(grabColor, c);
	#elif FT_DIFFERENCE_BLEND
		c = swf_difference(grabColor, c);
	#elif FT_INVERT_BLEND
		c = swf_invert(grabColor, c);
	#elif FT_OVERLAY_BLEND
		c = swf_overlay(grabColor, c);
	#elif FT_HARDLIGHT_BLEND
		c = swf_hardlight(grabColor, c);
	#endif

	c.rgb *= c.a;
	return c;
}

#endif // SWF_BASE_CG_INCLUDED