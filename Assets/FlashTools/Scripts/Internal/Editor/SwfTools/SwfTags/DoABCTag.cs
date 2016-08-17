using FlashTools.Internal.SwfTools.SwfAvm2;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class DoAbcTag : SwfTagBase {
		public bool       ExecuteImmediately;
		public string     Name;
		public SwfAbcFile AbcFile;

		public override SwfTagType TagType {
			get { return SwfTagType.DoAbc; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return "DoAbc.";
		}

		public static DoAbcTag Create(SwfStreamReader reader) {
			const int kDoAbcLazyInitializeFlag = 1;
			var flags    = reader.ReadUInt32();
			var name     = reader.ReadString();
			var abc_file = SwfAbcFile.Read(reader);
			return new DoAbcTag{
				ExecuteImmediately = (flags & kDoAbcLazyInitializeFlag) == 0,
				Name               = name,
				AbcFile            = abc_file};
		}
	}
}