namespace FlashTools.Internal.SwfTools.SwfTypes {
	struct SwfRGBA {
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public static SwfRGBA Read(SwfStreamReader reader) {
			var rgba = new SwfRGBA();
			rgba.R   = reader.ReadByte();
			rgba.G   = reader.ReadByte();
			rgba.B   = reader.ReadByte();
			rgba.A   = reader.ReadByte();
			return rgba;
		}

		public override string ToString() {
			return string.Format(
				"SwfRGBA. R: {0}, G: {1}, B: {2}, A: {3}",
				R, G, B, A);
		}
	}
}