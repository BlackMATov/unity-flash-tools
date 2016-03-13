using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using FlashTools.Internal.SwfTools;
using FlashTools.Internal.SwfTools.SwfTags;

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
			var decoder = new SwfDecoder(swf_asset);
			Debug.LogWarningFormat(
				"OriginalHeader: {0}",
				decoder.OriginalHeader);
			Debug.LogWarningFormat(
				"UncompressedHeader: {0}",
				decoder.UncompressedHeader);
			Debug.LogWarningFormat(
				"Tags: {0}",
				decoder.Tags.Count);
			foreach ( var tag in decoder.Tags ) {
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
	}
}
