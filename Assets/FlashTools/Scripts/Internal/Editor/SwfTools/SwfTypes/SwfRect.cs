using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfRect {
		public float XMin;
		public float XMax;
		public float YMin;
		public float YMax;

		public static SwfRect Read(SwfStreamReader reader) {
			var rect  = new SwfRect();
			var bits  = reader.ReadUnsignedBits(5);
			rect.XMin = reader.ReadSignedBits(bits) / 20.0f;
			rect.XMax = reader.ReadSignedBits(bits) / 20.0f;
			rect.YMin = reader.ReadSignedBits(bits) / 20.0f;
			rect.YMax = reader.ReadSignedBits(bits) / 20.0f;
			reader.AlignToByte();
			return rect;
		}

		public override string ToString() {
			return string.Format(
				"SwfRect. " +
				"XMin: {0}, XMax: {1}, YMin: {2}, YMax: {3}",
				XMin, XMax, YMin, YMax);
		}

		public Rect ToUnityRect() {
			return Rect.MinMaxRect(XMin, YMin, XMax, YMax);
		}
	}
}