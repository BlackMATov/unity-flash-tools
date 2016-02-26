using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

namespace FlashTools.Internal {
	public class FlashAnimAssetPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(
			string[] imported_assets, string[] deleted_assets,
			string[] moved_assets, string[] moved_from_asset_paths)
		{
			var asset_paths = imported_assets
				.Where(p => Path.GetExtension(p).ToLower().Equals(".asset"));
			foreach ( var asset_path in asset_paths ) {
				var asset = AssetDatabase.LoadAssetAtPath<FlashAnimAsset>(asset_path);
				if ( asset ) {
					FaAssetProcess(asset_path, asset);
				}
			}
		}

		static void FaAssetProcess(string fa_asset_path, FlashAnimAsset fa_asset) {
			try {
				if ( !MarkAllBitmapsReadable(fa_asset_path, fa_asset) ) {
					AssetDatabase.ImportAsset(fa_asset_path, ImportAssetOptions.ForceUpdate);
					return;
				}
				RemoveDuplicatedBitmaps(fa_asset_path, fa_asset);
				if ( !PackBitmapsAtlas(fa_asset_path, fa_asset) ) {
					AssetDatabase.ImportAsset(fa_asset_path, ImportAssetOptions.ForceUpdate);
					return;
				}
			} catch ( Exception e ) {
				Debug.LogErrorFormat("Postprocess flash anim asset error: {0}", e.Message);
			}
		}

		static bool MarkAllBitmapsReadable(string fa_asset_path, FlashAnimAsset fa_asset) {
			return FoldBitmapImporters(fa_asset_path, fa_asset.Data, true, (acc, importer) => {
				var readable = importer.isReadable;
				if ( !readable ) {
					importer.isReadable = true;
					AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
				}
				return readable && acc;
			});
		}

		static void RemoveDuplicatedBitmaps(string fa_asset_path, FlashAnimAsset fa_asset) {
			var bitmaps = fa_asset.Data.Library.Bitmaps;
			for ( var i = 0; i < bitmaps.Count; ++i ) {
				for ( var j = i + 1; j < bitmaps.Count; ++j ) {
					var fst_bitmap = bitmaps[i];
					var snd_bitmap = bitmaps[j];
					if ( IsBitmapsExistsAndDifferentButEqual(fa_asset_path, fst_bitmap, snd_bitmap) ) {
						var bitmap_importer = GetBitmapImporter(fa_asset_path, snd_bitmap);
						if ( AssetDatabase.DeleteAsset(bitmap_importer.assetPath) ) {
							snd_bitmap.CopyDataFrom(fst_bitmap);
							EditorUtility.SetDirty(fa_asset);
						}
					}
				}
			}
		}

		static bool PackBitmapsAtlas(string fa_asset_path, FlashAnimAsset fa_asset) {
			var textures = fa_asset.Data.Library.Bitmaps
				.Select(bitmap_data => GetBitmapTexture(fa_asset_path, bitmap_data))
				.ToList();
			var atlas       = new Texture2D(0, 0);
			var atlas_path  = GetBitmapsAtlasPath(fa_asset_path);
			var atlas_rects = atlas.PackTextures(
				textures.ToArray(), fa_asset.AtlasPadding, fa_asset.MaxAtlasSize);
			File.WriteAllBytes(atlas_path, atlas.EncodeToPNG());
			GameObject.DestroyImmediate(atlas);
			AssetDatabase.ImportAsset(atlas_path, ImportAssetOptions.ForceUpdate);
			for ( var i = 0; i < textures.Count; ++i ) {
				var bitmap_data        = fa_asset.Data.Library.Bitmaps[i];
				bitmap_data.RealSize   = new Vector2(textures[i].width, textures[i].height);
				bitmap_data.SourceRect = atlas_rects[i];
			}
			fa_asset.Data.Atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlas_path);
			EditorUtility.SetDirty(fa_asset);
			return fa_asset.Data.Atlas != null;
		}

		// -----------------------------
		// Common
		// -----------------------------

		static T FoldBitmapAssetPaths<T>(
			string fa_asset_path, FlashAnimData data,
			T init, Func<T, string, FlashAnimBitmapData, T> act)
		{
			return data.Library.Bitmaps.Aggregate(init, (acc, bitmap_data) => {
				var asset_path = GetBitmapAssetPath(fa_asset_path, bitmap_data);
				return act(acc, asset_path, bitmap_data);
			});
		}

		static T FoldBitmapImporters<T>(
			string fa_asset_path, FlashAnimData data,
			T init, Func<T, TextureImporter, T> act)
		{
			return FoldBitmapAssetPaths(fa_asset_path, data, init, (acc, path, bitmap_data) => {
				var importer = GetBitmapImporter(fa_asset_path, bitmap_data);
				return act(acc, importer);
			});
		}

		static string GetBitmapAssetPath(string fa_asset_path, FlashAnimBitmapData bitmap_data) {
			return Path.Combine(
				Path.GetDirectoryName(fa_asset_path),
				bitmap_data.ImageSource);
		}

		static Texture2D GetBitmapTexture(string fa_asset_path, FlashAnimBitmapData bitmap_data) {
			var asset_path = GetBitmapAssetPath(fa_asset_path, bitmap_data);
			var texture    = AssetDatabase.LoadAssetAtPath<Texture2D>(asset_path);
			if ( !texture ) {
				throw new UnityException(string.Format(
					"bitmap ({0}) texture not found ({1})",
					bitmap_data.Id, asset_path));
			}
			return texture;
		}

		static Texture2D FindBitmapTexture(string fa_asset_path, FlashAnimBitmapData bitmap_data) {
			try {
				return GetBitmapTexture(fa_asset_path, bitmap_data);
			} catch ( Exception ) {
				return null;
			}
		}

		static TextureImporter GetBitmapImporter(string fa_asset_path, FlashAnimBitmapData bitmap_data) {
			var asset_path = GetBitmapAssetPath(fa_asset_path, bitmap_data);
			var importer   = AssetImporter.GetAtPath(asset_path) as TextureImporter;
			if ( !importer ) {
				throw new UnityException(string.Format(
					"bitmap ({0}) texture importer not found ({1})",
					bitmap_data.Id, asset_path));
			}
			return importer;
		}

		static TextureImporter FindBitmapImporter(string fa_asset_path, FlashAnimBitmapData bitmap_data) {
			try {
				return GetBitmapImporter(fa_asset_path, bitmap_data);
			} catch ( Exception ) {
				return null;
			}
		}

		static bool IsBitmapsExistsAndDifferentButEqual(
			string fa_asset_path,
			FlashAnimBitmapData bitmap_data_a, FlashAnimBitmapData bitmap_data_b)
		{
			if ( bitmap_data_a.ImageSource == bitmap_data_b.ImageSource ) {
				return false;
			}
			var texture_a = FindBitmapTexture(fa_asset_path, bitmap_data_a);
			var texture_b = FindBitmapTexture(fa_asset_path, bitmap_data_b);
			if ( !texture_a || !texture_b ) {
				return false;
			}
			if ( texture_a.width != texture_b.width || texture_a.height != texture_b.height ) {
				return false;
			}
			var tex_data_a = texture_a.GetPixels32();
			var tex_data_b = texture_a.GetPixels32();
			return tex_data_a.SequenceEqual(tex_data_b);
		}

		static string GetBitmapsAtlasPath(string fa_asset_path) {
			return Path.ChangeExtension(fa_asset_path, "atlas.png");
		}
	}
}
