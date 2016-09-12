using UnityEngine;

namespace FlashTools.Internal {
	public static class SwfUtils {

		const uint Pow2_14 = (1 << 14); // 16 384
		const uint Pow2_15 = (1 << 15); // 32 768
		const uint Pow2_16 = (1 << 16); // 65 536

		//
		//
		//

		public static uint PackUV(float u, float v) {
			var uu = (uint)(Mathf.Clamp01(u) * Pow2_14);
			var vv = (uint)(Mathf.Clamp01(v) * Pow2_14);
			return (uu << 16) + vv;
		}

		public static Vector2 UnpackUV(uint v) {
			var uu = ((v >> 16) & 0xFFFF);
			var vv = (v & 0xFFFF);
			return new Vector2((float)uu / Pow2_14, (float)vv / Pow2_14);
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
			return (ushort)Mathf.Clamp(v * 20.0f, 0.0f, Pow2_16 - 1);
		}

		public static float UnpackFloatCoordFromUShort(ushort v) {
			return (float)(v / 20.0f);
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