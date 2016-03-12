namespace FlashTools.Internal.SwfTags {
	class UnknownTag : SwfTagBase {
		public int TagId;

		public override SwfTagType TagType {
			get { return SwfTagType.Unknown; }
		}

		public override string ToString() {
			return string.Format(
				"TagId: {0}", TagId);
		}

		public static UnknownTag Create(int tag_id) {
			var tag = new UnknownTag();
			tag.TagId = tag_id;
			return tag;
		}
	}
}
