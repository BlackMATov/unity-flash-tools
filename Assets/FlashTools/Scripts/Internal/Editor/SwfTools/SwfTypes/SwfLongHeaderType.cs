namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfLongHeader {
		public SwfShortHeader ShortHeader;
		public SwfRect        FrameSize;
		public float          FrameRate;
		public ushort         FrameCount;

		public static SwfLongHeader Read(SwfStreamReader reader) {
			var header         = new SwfLongHeader();
			header.ShortHeader = SwfShortHeader.Read(reader);
			header.FrameSize   = SwfRect.Read(reader);
			header.FrameRate   = reader.ReadFixedPoint8();
			header.FrameCount  = reader.ReadUInt16();
			return header;
		}

		public override string ToString() {
			return string.Format(
				"SwfLongHeader. " +
				"Format: {0}, Version: {1}, FileLength: {2}, " +
				"FrameSize: {3}, FrameRate: {4}, FrameCount: {5}",
				ShortHeader.Format, ShortHeader.Version, ShortHeader.FileLength,
				FrameSize, FrameRate, FrameCount);
		}
	}
}