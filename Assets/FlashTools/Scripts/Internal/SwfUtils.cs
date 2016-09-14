using UnityEngine;

namespace FlashTools.Internal {
	public static class SwfUtils {

		public const float UVPrecision        = 1.0f / 16384.0f;
		public const float FColorPrecision    = 1.0f / 512.0f;

		const ushort       UShortMax          = ushort.MaxValue;
		const float        InvFColorPrecision = 1.0f / FColorPrecision;

		//
		//
		//

		public static uint PackBytesToUInt(
			byte b0, byte b1, byte b2, byte b3)
		{
			var bb0 = (uint)b0;
			var bb1 = (uint)b1;
			var bb2 = (uint)b2;
			var bb3 = (uint)b3;
			return (bb0 << 24) + (bb1 << 16) + (bb2 << 8) + bb3;
		}

		public static void UnpackBytesFromUInt(
			uint pack,
			out byte b0, out byte b1, out byte b2, out byte b3)
		{
			b0 = (byte)((pack >> 24) & 0xFF);
			b1 = (byte)((pack >> 16) & 0xFF);
			b2 = (byte)((pack >>  8) & 0xFF);
			b3 = (byte)((pack      ) & 0xFF);
		}

		//
		//
		//

		public static uint PackUShortsToUInt(ushort x, ushort y) {
			var xx = (uint)x;
			var yy = (uint)y;
			return (xx << 16) + yy;
		}

		public static void UnpackUShortsFromUInt(
			uint pack,
			out ushort x, out ushort y)
		{
			x = (ushort)((pack >> 16) & 0xFFFF);
			y = (ushort)((pack      ) & 0xFFFF);
		}

		//
		//
		//
		public static uint PackUV(Vector2 uv) {
			return PackUV(uv.x, uv.y);
		}

		public static uint PackUV(float u, float v) {
			var uu = (uint)(Mathf.Clamp01(u) * UShortMax);
			var vv = (uint)(Mathf.Clamp01(v) * UShortMax);
			return (uu << 16) + vv;
		}

		public static void UnpackUV(uint pack, out float u, out float v) {
			u = (float)((pack >> 16) & 0xFFFF) / UShortMax;
			v = (float)((pack      ) & 0xFFFF) / UShortMax;
		}

		//
		//
		//

		public static ushort PackFloatColorToUShort(float v) {
			return (ushort)Mathf.Clamp(
				v * (1.0f / FColorPrecision),
				short.MinValue,
				short.MaxValue);
		}

		public static float UnpackFloatColorFromUShort(ushort pack) {
			return (short)pack / InvFColorPrecision;
		}

		//
		//
		//

		public static void PackFColorToUInts(
			Color v,
			out uint pack0, out uint pack1)
		{
			PackFColorToUInts(v.r, v.g, v.b, v.a, out pack0, out pack1);
		}

		public static void PackFColorToUInts(
			Vector4 v,
			out uint pack0, out uint pack1)
		{
			PackFColorToUInts(v.x, v.y, v.z, v.w, out pack0, out pack1);
		}

		public static void PackFColorToUInts(
			float v0, float v1, float v2, float v3,
			out uint pack0, out uint pack1)
		{
			pack0 = PackUShortsToUInt(
				PackFloatColorToUShort(v0),
				PackFloatColorToUShort(v1));
			pack1 = PackUShortsToUInt(
				PackFloatColorToUShort(v2),
				PackFloatColorToUShort(v3));
		}

		public static void UnpackFColorFromUInts(
			uint pack0, uint pack1,
			out float c0, out float c1, out float c2, out float c3)
		{
			c0 = (short)((pack0 >> 16) & 0xFFFF) / InvFColorPrecision;
			c1 = (short)((pack0      ) & 0xFFFF) / InvFColorPrecision;
			c2 = (short)((pack1 >> 16) & 0xFFFF) / InvFColorPrecision;
			c3 = (short)((pack1      ) & 0xFFFF) / InvFColorPrecision;
		}
	}
}