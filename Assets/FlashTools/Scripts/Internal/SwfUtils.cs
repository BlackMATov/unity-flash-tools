using UnityEngine;

namespace FlashTools.Internal {
	public static class SwfUtils {

		const uint Pow2_14 = (1 << 14); // 16 384

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
	}
}