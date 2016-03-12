namespace FlashTools.Internal.SwfTags {
	class DefineBitsLossless2Tag : SwfTagBase {
		public ushort CharacterId;
		public byte   BitmapFormat;
		public ushort BitmapWidth;
		public ushort BitmapHeight;
		public byte   BitmapColorTableSize;
		public byte[] ZlibBitmapData;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineBitsLossless2; }
		}

		public override string ToString() {
			return string.Format(
				"DefineBitsLossless2Tag. " +
				"CharacterId: {0}, BitmapFormat: {1}, Width: {2}, Height: {3}",
				CharacterId, BitmapFormat, BitmapWidth, BitmapHeight);
		}

		public static DefineBitsLossless2Tag Create(SwfStreamReader reader) {
			var tag          = new DefineBitsLossless2Tag();
			tag.CharacterId  = reader.Reader.ReadUInt16();
			tag.BitmapFormat = reader.Reader.ReadByte();
			tag.BitmapWidth  = reader.Reader.ReadUInt16();
			tag.BitmapHeight = reader.Reader.ReadUInt16();
			if ( tag.BitmapFormat == 3 ) {
				tag.BitmapColorTableSize = reader.Reader.ReadByte();
			}
			tag.ZlibBitmapData = reader.ReadRest();
			return tag;
		}
	}
}