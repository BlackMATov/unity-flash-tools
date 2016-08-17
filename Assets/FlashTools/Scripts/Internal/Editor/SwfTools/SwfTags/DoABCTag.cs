using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class DoABCTag : SwfTagBase {
		public bool       ExecuteImmediately;
		public string     Name;
		public SwfABCFile ABCFile;

		public override SwfTagType TagType {
			get { return SwfTagType.DoABC; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return "DoABC.";
		}

		public static DoABCTag Create(SwfStreamReader reader) {
			const int kDoAbcLazyInitializeFlag = 1;
			var flags    = reader.ReadUInt32();
			var name     = reader.ReadString();
			var abc_file = SwfABCFile.Read(reader);
			return new DoABCTag{
				ExecuteImmediately = (flags & kDoAbcLazyInitializeFlag) == 0,
				Name               = name,
				ABCFile            = abc_file};
		}
	}
}