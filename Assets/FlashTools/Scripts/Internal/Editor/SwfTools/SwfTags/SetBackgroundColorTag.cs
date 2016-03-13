using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools.SwfTags {
	class SetBackgroundColorTag : SwfTagBase {
		public SwfRGB BackgroundColor;

		public override SwfTagType TagType {
			get { return SwfTagType.SetBackgroundColor; }
		}

		public override string ToString() {
			return string.Format(
				"SetBackgroundColorTag. " +
				"BackgroundColor: {0}",
				BackgroundColor);
		}

		public static SetBackgroundColorTag Create(SwfStreamReader reader) {
			var tag             = new SetBackgroundColorTag();
			tag.BackgroundColor = SwfRGB.Read(reader);
			return tag;
		}
	}
}