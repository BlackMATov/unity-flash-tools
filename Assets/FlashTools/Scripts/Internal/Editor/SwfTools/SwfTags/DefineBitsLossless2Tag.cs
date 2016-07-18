using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class DefineBitsLossless2Tag : SwfTagBase {
		public ushort CharacterId;
		public byte   BitmapFormat;
		public ushort BitmapWidth;
		public ushort BitmapHeight;
		public byte   BitmapColorTableSize;
		public byte[] ZlibBitmapData;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineBitsLossless2; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return string.Format(
				"DefineBitsLossless2Tag. " +
				"CharacterId: {0}, BitmapFormat: {1}, Width: {2}, Height: {3}",
				CharacterId, BitmapFormat, BitmapWidth, BitmapHeight);
		}

		public static DefineBitsLossless2Tag Create(SwfStreamReader reader) {
			var tag          = new DefineBitsLossless2Tag();
			tag.CharacterId  = reader.ReadUInt16();
			tag.BitmapFormat = reader.ReadByte();
			tag.BitmapWidth  = reader.ReadUInt16();
			tag.BitmapHeight = reader.ReadUInt16();
			if ( tag.BitmapFormat == 3 ) {
				tag.BitmapColorTableSize = reader.ReadByte();
			}
			tag.ZlibBitmapData = reader.ReadRest();
			return tag;
		}

		public byte[] ToARGB32() {
			var result = new byte[BitmapWidth * BitmapHeight * 4];
			var swf_reader = new SwfStreamReader(
				SwfStreamReader.DecompressZBytes(ZlibBitmapData));
			if ( BitmapFormat == 5 ) {
				for ( var i = 0; i < BitmapWidth * BitmapHeight; ++i ) {
					var pix32 = swf_reader.ReadUInt32();
					result[i * 4 + 0] = (byte)((pix32      ) & 0xFF);
					result[i * 4 + 1] = (byte)((pix32 >>  8) & 0xFF);
					result[i * 4 + 2] = (byte)((pix32 >> 16) & 0xFF);
					result[i * 4 + 3] = (byte)((pix32 >> 24) & 0xFF);
				}
			} else {
				//TODO: IMPLME
				throw new UnityException(string.Format(
					"Unsupported DefineBitsLossless2 Format: {0}", BitmapFormat));
			}
			return result;
		}
	}
}