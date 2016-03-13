namespace FlashTools.Internal.SwfTools.SwfTags {
	class FileAttributesTag : SwfTagBase {
		public override SwfTagType TagType {
			get { return SwfTagType.FileAttributes; }
		}

		public override string ToString() {
			return "FileAttributesTag.";
		}

		public static FileAttributesTag Create(SwfStreamReader reader) {
			return new FileAttributesTag();
		}
	}
}