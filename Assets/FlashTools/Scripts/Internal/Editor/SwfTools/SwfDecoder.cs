﻿using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTags;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools {
	public class SwfDecoder {
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
				var rest_stream = SwfStreamReader.DecompressZBytes(
					raw_reader.ReadRest());
				var new_short_header = new SwfShortHeader{
					Format     = "FWS",
					Version    = OriginalHeader.Version,
					FileLength = OriginalHeader.FileLength};
				var uncompressed_stream = new MemoryStream();
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
				var tag = SwfTagBase.Read(reader);
				if ( tag.TagType == SwfTagType.End ) {
					break;
				}
				Tags.Add(tag);
			}
		}
	}
}