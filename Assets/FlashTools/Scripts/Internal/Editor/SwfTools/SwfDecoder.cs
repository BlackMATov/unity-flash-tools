using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTags;
using Ionic.Zlib;

namespace FlashTools.Internal.SwfTools {
	class SwfDecoder {
		public SwfShortHeader   OriginalHeader;
		public SwfLongHeader    UncompressedHeader;
		public List<SwfTagBase> Tags = new List<SwfTagBase>();

		public SwfDecoder(string swf_path) {
			var raw_data            = File.ReadAllBytes(swf_path);
			var uncompressed_stream = DecompressSwfData(raw_data);
			DecodeSwf(new SwfStreamReader(uncompressed_stream));
		}

		MemoryStream DecompressSwfData(byte[] raw_swf_data) {
			var raw_reader = new SwfStreamReader(raw_swf_data);
			OriginalHeader = SwfShortHeader.Read(raw_reader);
			switch ( OriginalHeader.Format ) {
			case "FWS":
				return new MemoryStream(raw_swf_data);
			case "CWS":
				var rest_stream      = DecompressZBytes(raw_reader.ReadRest());
				var new_short_header = new SwfShortHeader{
					Format     = "FWS",
					Version    = OriginalHeader.Version,
					FileLength = OriginalHeader.FileLength
				};
				var uncompressed_stream = new MemoryStream((int)OriginalHeader.FileLength);
				new_short_header.Write(uncompressed_stream);
				rest_stream.WriteTo(uncompressed_stream);
				uncompressed_stream.Position = 0;
				return uncompressed_stream;
			default:
				throw new UnityException(string.Format(
					"Unsupported swf format: {0}", OriginalHeader.Format));
			}
		}

		void DecodeSwf(SwfStreamReader reader) {
			UncompressedHeader = SwfLongHeader.Read(reader);
			while ( !reader.IsEOF ) {
				Tags.Add(SwfTagBase.Read(reader));
			}
		}

		static public MemoryStream DecompressZBytes(byte[] compressed_bytes) {
			var target     = new MemoryStream();
			var zip_stream = new ZlibStream(target, CompressionMode.Decompress);
			zip_stream.Write(compressed_bytes, 0, compressed_bytes.Length);
			target.Position = 0;
			return target;
		}
	}
}