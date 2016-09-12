using UnityEngine;

namespace FlashTools.Internal {
	public static class SwfUtils {

		public const float CoordPrecision = 0.05f;
		public const float ColorPrecision = 0.002f;

		public static uint PackUV(float u, float v) {
			var uu = (uint)(Mathf.Clamp01(u) * ushort.MaxValue);
			var vv = (uint)(Mathf.Clamp01(v) * ushort.MaxValue);
			return (uu << 16) + vv;
		}

		public static Vector2 UnpackUV(uint v) {
			var uu = ((v >> 16) & 0xFFFF);
			var vv = (v & 0xFFFF);
			return new Vector2(
				(float)uu / ushort.MaxValue,
				(float)vv / ushort.MaxValue);
		}

		//
		//
		//

		public static ushort PackFloatColorToUShort(float v) {
			return (ushort)Mathf.Clamp(
				v * (1.0f / ColorPrecision),
				short.MinValue,
				short.MaxValue);
		}

		public static float UnpackFloatColorFromUShort(ushort v) {
			return (short)v / (1.0f / ColorPrecision);
		}

		//
		//
		//

		public static void PackColorToUInts(Color v, out uint u0, out uint u1) {
			PackColorToUInts(v.r, v.g, v.b, v.a, out u0, out u1);
		}

		public static void PackColorToUInts(Vector4 v, out uint u0, out uint u1) {
			PackColorToUInts(v.x, v.y, v.z, v.w, out u0, out u1);
		}

		public static void PackColorToUInts(
			float v0, float v1, float v2, float v3,
			out uint u0, out uint u1)
		{
			var s0 = PackFloatColorToUShort(v0);
			var s1 = PackFloatColorToUShort(v1);
			var s2 = PackFloatColorToUShort(v2);
			var s3 = PackFloatColorToUShort(v3);
			u0 = PackUShortsToUInt(s0, s1);
			u1 = PackUShortsToUInt(s2, s3);
		}

		public static void UnpackColorFromUInts(uint u0, uint u1, out Color v) {
			ushort s0, s1, s2, s3;
			UnpackUShortsFromUInt(u0, out s0, out s1);
			UnpackUShortsFromUInt(u1, out s2, out s3);
			v = new Color(
				UnpackFloatColorFromUShort(s0),
				UnpackFloatColorFromUShort(s1),
				UnpackFloatColorFromUShort(s2),
				UnpackFloatColorFromUShort(s3));
		}

		public static void UnpackColorFromUInts(uint u0, uint u1, out Vector4 v) {
			ushort s0, s1, s2, s3;
			UnpackUShortsFromUInt(u0, out s0, out s1);
			UnpackUShortsFromUInt(u1, out s2, out s3);
			v = new Vector4(
				UnpackFloatColorFromUShort(s0),
				UnpackFloatColorFromUShort(s1),
				UnpackFloatColorFromUShort(s2),
				UnpackFloatColorFromUShort(s3));
		}

		//
		//
		//

		public static uint PackUShortsToUInt(ushort x, ushort y) {
			var xx = (uint)x;
			var yy = (uint)y;
			return (xx << 16) + yy;
		}

		public static void UnpackUShortsFromUInt(uint v, out ushort x, out ushort y) {
			var xx = ((v >> 16) & 0xFFFF);
			var yy = (v & 0xFFFF);
			x = (ushort)xx;
			y = (ushort)yy;
		}

		//
		//
		//

		public static ushort PackFloatCoordToUShort(float v) {
			return (ushort)Mathf.Clamp(
				v * (1.0f / CoordPrecision),
				0,
				ushort.MaxValue);
		}

		public static float UnpackFloatCoordFromUShort(ushort v) {
			return v / (1.0f / CoordPrecision);
		}

		//
		//
		//

		public static uint PackCoordsToUInt(Vector2 v) {
			return PackUShortsToUInt(
				PackFloatCoordToUShort(v.x),
				PackFloatCoordToUShort(v.y));
		}

		public static Vector2 UnpackCoordsFromUInt(uint v) {
			ushort sx, sy;
			UnpackUShortsFromUInt(v, out sx, out sy);
			return new Vector2(
				SwfUtils.UnpackFloatCoordFromUShort(sx),
				SwfUtils.UnpackFloatCoordFromUShort(sy));
		}
	}
}