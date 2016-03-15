using UnityEngine;
using System.IO;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	struct SwfShortHeader {
		public string Format;
		public byte   Version;
		public uint   FileLength;

		public static SwfShortHeader Read(SwfStreamReader reader) {
			var header        = new SwfShortHeader();
			header.Format     = new string(reader.ReadChars(3));
			header.Version    = reader.ReadByte();
			header.FileLength = reader.ReadUInt32();
			return header;
		}

		public void Write(Stream stream) {
			if ( Format == null || Format.Length != 3 ) {
				throw new UnityException(string.Format(
					"Incorrect SwfShortHeader Format: {0}",
					Format));
			}
			stream.WriteByte((byte)Format[0]);
			stream.WriteByte((byte)Format[1]);
			stream.WriteByte((byte)Format[2]);
			stream.WriteByte(Version);
			stream.WriteByte((byte)((FileLength >>  0) & 0xFF));
			stream.WriteByte((byte)((FileLength >>  8) & 0xFF));
			stream.WriteByte((byte)((FileLength >> 16) & 0xFF));
			stream.WriteByte((byte)((FileLength >> 24) & 0xFF));
		}

		public override string ToString() {
			return string.Format(
				"SwfShortHeader. " +
				"Format: {0}, Version: {1}, FileLength: {2}",
				Format, Version, FileLength);
		}
	}
}