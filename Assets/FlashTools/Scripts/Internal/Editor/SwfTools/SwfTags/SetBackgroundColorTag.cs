using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class SetBackgroundColorTag : SwfTagBase {
		public SwfColor BackgroundColor;

		public override SwfTagType TagType {
			get { return SwfTagType.SetBackgroundColor; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return string.Format(
				"SetBackgroundColorTag. " +
				"BackgroundColor: {0}",
				BackgroundColor);
		}

		public static SetBackgroundColorTag Create(SwfStreamReader reader) {
			var tag             = new SetBackgroundColorTag();
			tag.BackgroundColor = SwfColor.Read(reader, false);
			return tag;
		}
	}
}