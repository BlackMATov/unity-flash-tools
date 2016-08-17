using System.Collections.Generic;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class SymbolClassTag : SwfTagBase {
		public struct SymbolData {
			public ushort Tag;
			public string Name;
		}

		public List<SymbolData> Symbols;

		public override SwfTagType TagType {
			get { return SwfTagType.SymbolClass; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return string.Format(
				"SymbolClass." +
				"Symbols: {0}",
				Symbols.Count);
		}

		public static SymbolClassTag Create(SwfStreamReader reader) {
			var symbol_count = reader.ReadUInt16();
			var symbols      = new List<SymbolData>(symbol_count);
			for ( var i = 0; i < symbols.Capacity; ++i ) {
				symbols.Add(new SymbolData{
					Tag  = reader.ReadUInt16(),
					Name = reader.ReadString()
				});
			}
			return new SymbolClassTag{
				Symbols = symbols
			};
		}
	}
}