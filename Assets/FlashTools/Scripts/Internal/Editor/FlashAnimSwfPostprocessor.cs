using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Ionic.Zlib;
using FlashTools.Internal.SwfTags;

namespace FlashTools.Internal {
	public class FlashAnimSwfPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(
			string[] imported_assets, string[] deleted_assets,
			string[] moved_assets, string[] moved_from_asset_paths)
		{
			var swf_asset_paths = imported_assets
				.Where(p => Path.GetExtension(p).ToLower().Equals(".swf"));
			foreach ( var swf_asset_path in swf_asset_paths ) {
				SwfAssetProcess(swf_asset_path);
			}
		}

		static void SwfAssetProcess(string swf_asset) {
			Debug.LogFormat("SwfAssetProcess: {0}", swf_asset);
			var raw_reader = new SwfStreamReader(swf_asset);
			var header = SwfHeader.Read(raw_reader);
			Debug.LogFormat("Header: {0}", header);
			SwfStreamReader swf_reader;
			switch ( header.Format ) {
			case "FWS":
				swf_reader = raw_reader;
				SwfRect.Read(swf_reader);
				swf_reader.ReadFixedPoint8();
				swf_reader.Reader.ReadUInt16();
				break;
			case "CWS":
				var mem = new MemoryStream();
				Decompress(new MemoryStream(raw_reader.ReadRest()), mem);
				swf_reader = new SwfStreamReader(mem);
				SwfRect.Read(swf_reader);
				swf_reader.ReadFixedPoint8();
				swf_reader.Reader.ReadUInt16();

				//var swf_rest = raw_reader.ReadRest();
				//var raw_rest = Decompress(swf_rest);
				//swf_reader = new SwfStreamReader(new MemoryStream(raw_rest));
				break;
			default:
				throw new UnityException(string.Format(
					"Unsupported swf format: {0}", header.Format));
			}
			while ( !swf_reader.IsEOF ) {
				var tag_data = SwfTagData.Read(swf_reader);
				var tag = SwfTagBase.Create(tag_data);
				if ( tag.TagType == SwfTagType.Unknown ) {
					Debug.LogWarningFormat("Tag: {0}", tag.ToString());
				} else {
					Debug.LogFormat("Tag: {0}", tag.ToString());
				}
			}
			//SwfBitmapsToAtlas(swf_asset, tags);
		}

		/*
		static void SwfBitmapsToAtlas(string swf_asset, List<SwfTagBase> tags) {
			var defines = tags
				.Where(p => p.TagType == SwfTagType.DefineBitsLossless2)
				.Select(p => p as DefineBitsLossless2Tag)
				.Where(p => p.BitmapFormat == 5);
			var textures = new List<Texture2D>();
			foreach ( var define in defines ) {
				var data = Decompress(define.ZlibBitmapData);
				var texture = new Texture2D(
					define.BitmapWidth, define.BitmapHeight,
					TextureFormat.ARGB32, false);
				texture.LoadRawTextureData(data);
				textures.Add(texture);
			}
			var atlas       = new Texture2D(0, 0);
			var atlas_path  = Path.ChangeExtension(swf_asset, ".png");
			atlas.PackTextures(textures.ToArray(), 1, 1024);
			File.WriteAllBytes(atlas_path, atlas.EncodeToPNG());
			GameObject.DestroyImmediate(atlas, true);
			AssetDatabase.ImportAsset(
				atlas_path,
				ImportAssetOptions.ForceUncompressedImport);
		}*/

		static void Decompress(Stream compressed, Stream target) {
			var zip = new ZlibStream(target, CompressionMode.Decompress);
			int readBytes;
			var buffer = new byte[512];
			do {
				readBytes = compressed.Read(buffer, 0, buffer.Length);
				zip.Write(buffer, 0, readBytes);
			} while (readBytes > 0);
			zip.Flush();
			target.Seek(0, SeekOrigin.Begin);
		}

		static byte[] Decompress(byte[] compressed) {
			var mem = new MemoryStream();
			Decompress(new MemoryStream(compressed), mem);
			return mem.ToArray();
		}
	}
}
