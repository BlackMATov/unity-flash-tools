using System.IO;

namespace SwfTools.SwfTypes {
	public struct SwfShortHeader {
		public string Format;
		public byte   Version;
		public uint   FileLength;

		public static SwfShortHeader Read(SwfStreamReader reader) {
			return CheckHeader(new SwfShortHeader{
				Format     = new string(reader.ReadChars(3)),
				Version    = reader.ReadByte(),
				FileLength = reader.ReadUInt32()});
		}

		public static void Write(SwfShortHeader header, Stream stream) {
			CheckHeader(header);
			stream.WriteByte((byte)header.Format[0]);
			stream.WriteByte((byte)header.Format[1]);
			stream.WriteByte((byte)header.Format[2]);
			stream.WriteByte(header.Version);
			stream.WriteByte((byte)((header.FileLength >>  0) & 0xFF));
			stream.WriteByte((byte)((header.FileLength >>  8) & 0xFF));
			stream.WriteByte((byte)((header.FileLength >> 16) & 0xFF));
			stream.WriteByte((byte)((header.FileLength >> 24) & 0xFF));
		}

		public override string ToString() {
			return string.Format(
				"SwfShortHeader. " +
				"Format: {0}, Version: {1}, FileLength: {2}",
				Format, Version, FileLength);
		}

		static SwfShortHeader CheckHeader(SwfShortHeader header) {
			if ( string.IsNullOrEmpty(header.Format) || (header.Format != "FWS" && header.Format != "CWS") ) {
				throw new System.Exception(string.Format(
					"Incorrect swf header format: {0}",
					header.Format));
			} else if ( header.Version != 28 ) {
				throw new System.Exception(string.Format(
					"Incorrect swf header version: {0}",
					header.Version));
			}
			return header;
		}
	}
}