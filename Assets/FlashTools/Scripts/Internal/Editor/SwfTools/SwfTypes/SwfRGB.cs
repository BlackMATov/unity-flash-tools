namespace FlashTools.Internal.SwfTools.SwfTypes {
	struct SwfRGB {
		public byte R;
		public byte G;
		public byte B;

		public static SwfRGB Read(SwfStreamReader reader) {
			var rgb = new SwfRGB();
			rgb.R   = reader.ReadByte();
			rgb.G   = reader.ReadByte();
			rgb.B   = reader.ReadByte();
			return rgb;
		}

		public override string ToString() {
			return string.Format(
				"SwfRGB. R: {0}, G: {1}, B: {2}",
				R, G, B);
		}
	}
}