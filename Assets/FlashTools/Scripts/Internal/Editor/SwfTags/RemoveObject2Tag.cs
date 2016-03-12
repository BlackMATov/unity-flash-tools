namespace FlashTools.Internal.SwfTags {
	class RemoveObject2Tag : SwfTagBase {
		public ushort Depth;

		public override SwfTagType TagType {
			get { return SwfTagType.RemoveObject2; }
		}

		public override string ToString() {
			return string.Format(
				"RemoveObject2Tag. " +
				"Depth: {0}",
				Depth);
		}

		public static RemoveObject2Tag Create(SwfStreamReader reader) {
			var tag   = new RemoveObject2Tag();
			tag.Depth = reader.Reader.ReadUInt16();
			return tag;
		}
	}
}