namespace FlashTools.Internal.SwfTools.SwfTags {
	class FrameLabelTag : SwfTagBase {
		public string Name;
		public byte   AnchorFlag;

		public override SwfTagType TagType {
			get { return SwfTagType.FrameLabel; }
		}

		public override string ToString() {
			return string.Format(
				"FrameLabelTag. " +
				"Name: {0}, AnchorFlag: {1}",
				Name, AnchorFlag);
		}

		public static FrameLabelTag Create(SwfStreamReader reader) {
			var tag = new FrameLabelTag();
			tag.Name = reader.ReadString();
			if ( !reader.IsEOF ) {
				tag.AnchorFlag = reader.ReadByte();
			}
			return tag;
		}
	}
}