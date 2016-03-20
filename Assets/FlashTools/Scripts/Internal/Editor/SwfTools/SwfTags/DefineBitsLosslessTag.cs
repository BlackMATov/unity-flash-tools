namespace FlashTools.Internal.SwfTools.SwfTags {
	public class DefineBitsLosslessTag : SwfTagBase {
		public ushort CharacterId;
		public byte   BitmapFormat;
		public ushort BitmapWidth;
		public ushort BitmapHeight;
		public byte   BitmapColorTableSize;
		public byte[] ZlibBitmapData;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineBitsLossless; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return string.Format(
				"DefineBitsLosslessTag. " +
				"CharacterId: {0}, BitmapFormat: {1}, Width: {2}, Height: {3}",
				CharacterId, BitmapFormat, BitmapWidth, BitmapHeight);
		}

		public static DefineBitsLosslessTag Create(SwfStreamReader reader) {
			var tag          = new DefineBitsLosslessTag();
			tag.CharacterId  = reader.ReadUInt16();
			tag.BitmapFormat = reader.ReadByte();
			tag.BitmapWidth  = reader.ReadUInt16();
			tag.BitmapHeight = reader.ReadUInt16();
			if ( tag.BitmapFormat == 3 ) {
				tag.BitmapColorTableSize = reader.ReadByte();
			}
			tag.ZlibBitmapData = reader.ReadRest();
			return tag;
		}
	}
}