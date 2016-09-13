using UnityEngine;

namespace FlashTools.Internal {
	public static class SwfUtils {

		public const float UVPrecision    = 0.0001f;
		public const float CoordPrecision = 0.05f;
		public const float ColorPrecision = 0.002f;

		//
		//
		//

		public static uint PackUShortsToUInt(ushort x, ushort y) {
			var xx = (uint)x;
			var yy = (uint)y;
			return (xx << 16) + yy;
		}

		public static void UnpackUShortsFromUInt(uint pack, out ushort x, out ushort y) {
			var xx = ((pack >> 16) & 0xFFFF);
			var yy = (pack & 0xFFFF);
			x = (ushort)xx;
			y = (ushort)yy;
		}

		//
		//
		//

		public static uint PackUV(float u, float v) {
			var uu = (uint)(Mathf.Clamp01(u) * ushort.MaxValue);
			var vv = (uint)(Mathf.Clamp01(v) * ushort.MaxValue);
			return (uu << 16) + vv;
		}

		public static void UnpackUV(uint pack, out float u, out float v) {
			var uu = ((pack >> 16) & 0xFFFF);
			var vv = (pack & 0xFFFF);
			u = (float)uu / ushort.MaxValue;
			v = (float)vv / ushort.MaxValue;
		}

		public static uint PackUV(Vector2 uv) {
			return PackUV(uv.x, uv.y);
		}

		public static Vector2 UnpackUV(uint pack) {
			float u, v;
			UnpackUV(pack, out u, out v);
			return new Vector2(u, v);
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

		public static float UnpackFloatColorFromUShort(ushort pack) {
			return (short)pack / (1.0f / ColorPrecision);
		}

		//
		//
		//

		public static void PackColorToUInts(
			Color v,
			out uint pack0, out uint pack1)
		{
			PackColorToUInts(v.r, v.g, v.b, v.a, out pack0, out pack1);
		}

		public static void PackColorToUInts(
			Vector4 v,
			out uint pack0, out uint pack1)
		{
			PackColorToUInts(v.x, v.y, v.z, v.w, out pack0, out pack1);
		}

		public static void PackColorToUInts(
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

		public static void UnpackColorFromUInts(
			uint pack0, uint pack1,
			out Color color)
		{
			ushort s0, s1, s2, s3;
			UnpackUShortsFromUInt(pack0, out s0, out s1);
			UnpackUShortsFromUInt(pack1, out s2, out s3);
			color = new Color(
				UnpackFloatColorFromUShort(s0),
				UnpackFloatColorFromUShort(s1),
				UnpackFloatColorFromUShort(s2),
				UnpackFloatColorFromUShort(s3));
		}

		public static void UnpackColorFromUInts(
			uint pack0, uint pack1,
			out Vector4 color)
		{
			ushort s0, s1, s2, s3;
			UnpackUShortsFromUInt(pack0, out s0, out s1);
			UnpackUShortsFromUInt(pack1, out s2, out s3);
			color = new Vector4(
				UnpackFloatColorFromUShort(s0),
				UnpackFloatColorFromUShort(s1),
				UnpackFloatColorFromUShort(s2),
				UnpackFloatColorFromUShort(s3));
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

		public static float UnpackFloatCoordFromUShort(ushort pack) {
			return pack / (1.0f / CoordPrecision);
		}

		//
		//
		//

		public static uint PackCoordsToUInt(float x, float y) {
			return PackUShortsToUInt(
				PackFloatCoordToUShort(x),
				PackFloatCoordToUShort(y));
		}

		public static void UnpackCoordsFromUInt(
			uint pack,
			out float x, out float y)
		{
			ushort sx, sy;
			UnpackUShortsFromUInt(pack, out sx, out sy);
			x = SwfUtils.UnpackFloatCoordFromUShort(sx);
			y = SwfUtils.UnpackFloatCoordFromUShort(sy);
		}

		public static uint PackCoordsToUInt(Vector2 v) {
			return PackCoordsToUInt(v.x, v.y);
		}

		public static Vector2 UnpackCoordsFromUInt(uint pack) {
			float x, y;
			UnpackCoordsFromUInt(pack, out x, out y);
			return new Vector2(x, y);
		}
	}
}