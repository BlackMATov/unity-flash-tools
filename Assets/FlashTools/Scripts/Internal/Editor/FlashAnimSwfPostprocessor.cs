using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Ionic.Zlib;

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
			var reader = new SwfStreamReader(swf_asset);
			var header = SwfHeader.Read(reader);
			Debug.LogFormat("Header: {0}", header);
			while ( !reader.IsEOF ) {
				var tag_data = SwfTagData.Read(reader);
				var tag = SwfTagBase.Create(tag_data);
				Debug.LogFormat("Tag: {0} - {1}", tag.TagType.ToString(), tag.ToString());
			}
		}
	}
}
