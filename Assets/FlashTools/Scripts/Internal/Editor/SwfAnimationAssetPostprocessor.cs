using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace FlashTools.Internal {
	public class SwfAnimationAssetPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(
			string[] imported_assets, string[] deleted_assets,
			string[] moved_assets, string[] moved_from_asset_paths)
		{
			var asset_paths = imported_assets
				.Where(p => Path.GetExtension(p).ToLower().Equals(".asset"));
			foreach ( var asset_path in asset_paths ) {
				var asset = AssetDatabase.LoadAssetAtPath<SwfAnimationAsset>(asset_path);
				if ( asset ) {
					AssetProcess(asset_path, asset);
				}
			}
		}

		static void AssetProcess(string asset_path, SwfAnimationAsset asset) {
			try {
				var new_asset = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAtlasPath(asset_path));
				if ( new_asset != asset.Atlas ) {
					asset.Atlas = new_asset;
					ConfigureAtlas(asset_path, asset);
					EditorUtility.SetDirty(asset);
					AssetDatabase.SaveAssets();
				}
			} catch ( Exception e ) {
				Debug.LogErrorFormat(
					"Postprocess swf animation asset error: {0}",
					e.Message);
			}
		}

		static void ConfigureAtlas(string asset_path, SwfAnimationAsset asset) {
			var meta_data      = new List<SpriteMetaData>();
			var atlas_importer = GetBitmapsAtlasImporter(asset_path);
			var atlas_size     = GetSizeFromTextureImporter(atlas_importer);
			var unique_bitmaps = asset.Data.Bitmaps;
			foreach ( var bitmap_data in unique_bitmaps ) {
				var meta_elem = new SpriteMetaData();
				meta_elem.name = bitmap_data.Id.ToString();
				meta_elem.rect = new Rect(
					bitmap_data.SourceRect.xMin   * atlas_size.x,
					bitmap_data.SourceRect.yMin   * atlas_size.y,
					bitmap_data.SourceRect.width  * atlas_size.x,
					bitmap_data.SourceRect.height * atlas_size.y);
				meta_data.Add(meta_elem);
				atlas_importer.spritesheet         = meta_data.ToArray();
				atlas_importer.textureType         = TextureImporterType.Sprite;
				atlas_importer.spriteImportMode    = SpriteImportMode.Multiple;
				atlas_importer.spritePixelsPerUnit = asset.PixelsPerUnit;
				AssetDatabase.ImportAsset(
					GetAtlasPath(asset_path),
					ImportAssetOptions.ForceUncompressedImport);
			}
		}

		static TextureImporter GetBitmapsAtlasImporter(string asset_path) {
			var atlas_path = GetAtlasPath(asset_path);
			var importer   = AssetImporter.GetAtPath(atlas_path) as TextureImporter;
			if ( !importer ) {
				throw new UnityException(string.Format(
					"atlas texture importer not found ({0})",
					atlas_path));
			}
			return importer;
		}

		static Vector2 GetSizeFromTextureImporter(TextureImporter importer) {
			var method_args = new object[2]{0,0};
			typeof(TextureImporter)
				.GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(importer, method_args);
			return new Vector2((int)method_args[0], (int)method_args[1]);
		}

		static string GetSwfPath(string asset_path) {
			return Path.ChangeExtension(asset_path, ".swf");
		}

		static string GetAtlasPath(string asset_path) {
			return Path.ChangeExtension(asset_path, ".png");
		}
	}
}
