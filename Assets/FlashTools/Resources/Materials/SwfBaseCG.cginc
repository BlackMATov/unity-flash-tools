#ifndef SWF_BASE_CG_INCLUDED
#define SWF_BASE_CG_INCLUDED

//
// blending functions
//

inline fixed4 swf_darken(fixed4 a, fixed4 b) {
	fixed4 r = min(a, b);
	r.a = b.a;
	return r;
}

inline fixed4 swf_difference(fixed4 a, fixed4 b) {
	fixed4 r = abs(a - b);
	r.a = b.a;
	return r;
}

inline fixed4 swf_invert(fixed4 a, fixed4 b) {
	fixed4 r = 1 - a;
	r.a = b.a;
	return r;
}

inline fixed4 swf_overlay(fixed4 a, fixed4 b) {
	fixed4 r = a > 0.5 ? 1.0 - 2.0 * (1.0 - a) * (1.0 - b) : 2.0 * a * b;
	r.a = b.a;
	return r;
}

inline fixed4 swf_hardlight(fixed4 a, fixed4 b) {
	fixed4 r = b > 0.5 ? 1.0 - (1.0 - a) * (1.0 - 2.0 * (b - 0.5)) : a * (2.0 * b);
	r.a = b.a;
	return r;
}

inline fixed4 grab_blend(sampler2D grab_tex, float4 screenpos, fixed4 c) {
	float2 grab_uv = screenpos.xy / screenpos.w;
	grab_uv.x = (grab_uv.x + 1.0) * .5;
	grab_uv.y = (grab_uv.y + 1.0) * .5;
#if UNITY_UV_STARTS_AT_TOP
	grab_uv.y = 1.0 - grab_uv.y;
#endif
	fixed4 grab_c = tex2D(grab_tex, grab_uv);
	#if SWF_DARKEN_BLEND
		c = swf_darken(grab_c, c);
	#elif SWF_DIFFERENCE_BLEND
		c = swf_difference(grab_c, c);
	#elif SWF_INVERT_BLEND
		c = swf_invert(grab_c, c);
	#elif SWF_OVERLAY_BLEND
		c = swf_overlay(grab_c, c);
	#elif SWF_HARDLIGHT_BLEND
		c = swf_hardlight(grab_c, c);
	#endif
	return c;
}

//
// structs
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
// vert functions
//

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

//
// frag functions
//

inline fixed4 swf_frag(swf_v2f_t IN) : SV_Target {
	fixed4 c = tex2D(_MainTex, IN.uv);
	if ( c.a > 0.01 ) {
		c = c * IN.mulcolor + IN.addcolor;
	}
	c.rgb *= c.a;
	return c;
}

inline fixed4 swf_grab_frag(swf_grab_v2f_t IN) : SV_Target {
	fixed4 c = tex2D(_MainTex, IN.uv);
	if ( c.a > 0.01 ) {
		c = c * IN.mulcolor + IN.addcolor;
	}
	c = grab_blend(_GrabTexture, IN.screenpos, c);
	c.rgb *= c.a;
	return c;
}

#endif // SWF_BASE_CG_INCLUDED