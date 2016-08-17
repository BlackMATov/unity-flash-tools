namespace FlashTools.Internal.SwfTools.SwfTags {
	public class UnknownTag : SwfTagBase {
		public int TagId;

		public override SwfTagType TagType {
			get { return SwfTagType.Unknown; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return string.Format(
				"UnknownTag. " +
				"TagId: {0}",
				TagId);
		}

		public static UnknownTag Create(int tag_id) {
			var tag = new UnknownTag();
			tag.TagId = tag_id;
			return tag;
		}
	}
}
