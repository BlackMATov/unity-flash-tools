using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfColor {
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public static SwfColor identity {
			get {
				return new SwfColor {
					R = byte.MaxValue,
					G = byte.MaxValue,
					B = byte.MaxValue,
					A = byte.MaxValue};
			}
		}

		public static SwfColor Read(SwfStreamReader reader, bool with_alpha) {
			var r = reader.ReadByte();
			var g = reader.ReadByte();
			var b = reader.ReadByte();
			var a = with_alpha ? reader.ReadByte() : byte.MaxValue;
			return new SwfColor{
				R = r,
				G = g,
				B = b,
				A = a};
		}

		public override string ToString() {
			return string.Format(
				"SwfColor. R: {0}, G: {1}, B: {2}, A: {3}",
				R, G, B, A);
		}

		public Color ToUnityColor() {
			return new Color(
				R / 255.0f,
				G / 255.0f,
				B / 255.0f,
				A / 255.0f);
		}

		public Color32 ToUnityColor32() {
			return new Color32(
				R,
				G,
				B,
				A);
		}
	}
}