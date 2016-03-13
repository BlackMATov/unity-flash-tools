namespace FlashTools.Internal.SwfTools.SwfTags {
	class EndTag : SwfTagBase {
		public override SwfTagType TagType {
			get { return SwfTagType.End; }
		}

		public override string ToString() {
			return "EndTag.";
		}

		public static EndTag Create(SwfStreamReader reader) {
			return new EndTag();
		}
	}
}