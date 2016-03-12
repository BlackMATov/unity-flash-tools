namespace FlashTools.Internal.SwfTags {
	class ShowFrameTag : SwfTagBase {
		public override SwfTagType TagType {
			get { return SwfTagType.ShowFrame; }
		}

		public override string ToString() {
			return "ShowFrameTag.";
		}

		public static ShowFrameTag Create(SwfStreamReader reader) {
			return new ShowFrameTag();
		}
	}
}