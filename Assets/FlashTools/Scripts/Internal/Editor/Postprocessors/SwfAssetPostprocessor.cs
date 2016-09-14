using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using FlashTools.Internal;

namespace FlashTools.Internal {
	public class SwfAssetPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(
			string[] imported_assets,
			string[] deleted_assets,
			string[] moved_assets,
			string[] moved_from_asset_paths)
		{
			var asset_paths = imported_assets
				.Where(p => Path.GetExtension(p).ToLower().Equals(".asset"));
			foreach ( var asset_path in asset_paths ) {
				var asset = AssetDatabase.LoadAssetAtPath<SwfAsset>(asset_path);
				if ( asset ) {
					SwfAssetProcess(asset);
				}
			}
		}

		static void SwfAssetProcess(SwfAsset asset) {
			try {
				var atlas_asset = LoadAtlasAsset(asset);
				if ( atlas_asset != asset.Atlas ) {
					asset.Atlas = atlas_asset;
					ConfigureAtlas(asset);
					ConfigureClips(asset);
					ConfigureAssetClips(asset);
					EditorUtility.SetDirty(asset);
					AssetDatabase.SaveAssets();
				}
			} catch ( Exception e ) {
				Debug.LogErrorFormat(
					"Postprocess swf asset error: {0}",
					e.Message);
				SwfEditorUtils.DeleteAssetWithDepends(asset);
			}
		}

		static Texture2D LoadAtlasAsset(SwfAsset asset) {
			return AssetDatabase.LoadAssetAtPath<Texture2D>(
				SwfEditorUtils.GetAtlasPathFromAsset(asset));
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureAtlas
		//
		// ---------------------------------------------------------------------

		static void ConfigureAtlas(SwfAsset asset) {
			var atlas_importer      = GetBitmapsAtlasImporter(asset);
			var atlas_importer_size = GetSizeFromTextureImporter(atlas_importer);
			atlas_importer.spritesheet = asset.Data.Bitmaps
				.Select(bitmap => new SpriteMetaData{
					name = bitmap.Id.ToString(),
					rect = new Rect(
						bitmap.SourceRect.xMin   * atlas_importer_size.x,
						bitmap.SourceRect.yMin   * atlas_importer_size.y,
						bitmap.SourceRect.width  * atlas_importer_size.x,
						bitmap.SourceRect.height * atlas_importer_size.y)})
				.ToArray();
			atlas_importer.textureType         = TextureImporterType.Sprite;
			atlas_importer.spriteImportMode    = SpriteImportMode.Multiple;
			atlas_importer.spritePixelsPerUnit = asset.Settings.PixelsPerUnit;
			atlas_importer.mipmapEnabled       = asset.Settings.GenerateMipMaps;
			atlas_importer.filterMode          = SwfAtlasFilterToImporterFilter(asset.Settings.AtlasTextureFilter);
			atlas_importer.textureFormat       = SwfAtlasFormatToImporterFormat(asset.Settings.AtlasTextureFormat);
			AssetDatabase.ImportAsset(SwfEditorUtils.GetAtlasPathFromAsset(asset));
		}

		static TextureImporter GetBitmapsAtlasImporter(SwfAsset asset) {
			var atlas_path     = SwfEditorUtils.GetAtlasPathFromAsset(asset);
			var atlas_importer = AssetImporter.GetAtPath(atlas_path) as TextureImporter;
			if ( !atlas_importer ) {
				throw new UnityException(string.Format(
					"atlas texture importer not found ({0})",
					atlas_path));
			}
			return atlas_importer;
		}

		static Vector2 GetSizeFromTextureImporter(TextureImporter importer) {
			var method_args = new object[2]{0,0};
			typeof(TextureImporter)
				.GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(importer, method_args);
			return new Vector2((int)method_args[0], (int)method_args[1]);
		}

		static FilterMode SwfAtlasFilterToImporterFilter(
			SwfSettingsData.AtlasFilter filter)
		{
			switch ( filter ) {
			case SwfSettingsData.AtlasFilter.Point:
				return FilterMode.Point;
			case SwfSettingsData.AtlasFilter.Bilinear:
				return FilterMode.Bilinear;
			case SwfSettingsData.AtlasFilter.Trilinear:
				return FilterMode.Trilinear;
			default:
				throw new UnityException(string.Format(
					"incorrect swf atlas filter ({0})",
					filter));
			}
		}

		static TextureImporterFormat SwfAtlasFormatToImporterFormat(
			SwfSettingsData.AtlasFormat format)
		{
			switch ( format ) {
			case SwfSettingsData.AtlasFormat.AutomaticCompressed:
				return TextureImporterFormat.AutomaticCompressed;
			case SwfSettingsData.AtlasFormat.Automatic16bit:
				return TextureImporterFormat.Automatic16bit;
			case SwfSettingsData.AtlasFormat.AutomaticTruecolor:
				return TextureImporterFormat.AutomaticTruecolor;
			case SwfSettingsData.AtlasFormat.AutomaticCrunched:
				return TextureImporterFormat.AutomaticCrunched;
			default:
				throw new UnityException(string.Format(
					"incorrect swf atlas format ({0})",
					format));
			}
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureClips
		//
		// ---------------------------------------------------------------------

		static void ConfigureClips(SwfAsset asset) {
			asset.Clips.Clear();
			foreach ( var symbol in asset.Data.Symbols ) {
				ConfigureClip(asset, symbol);
			}
		}

		static void ConfigureClip(SwfAsset asset, SwfSymbolData symbol) {
			var asset_path       = AssetDatabase.GetAssetPath(asset);
			var clip_asset_path  = Path.ChangeExtension(asset_path, symbol.Name + ".asset");
			var clip_asset       = SwfEditorUtils.LoadOrCreateAsset<SwfClipAsset>(clip_asset_path);
			clip_asset.Atlas     = asset.Atlas;
			clip_asset.Container = AssetDatabase.AssetPathToGUID(asset_path);
			clip_asset.FrameRate = asset.Data.FrameRate;
			clip_asset.Sequences = LoadClipSequences(asset, symbol);
			EditorUtility.SetDirty(clip_asset);
			asset.Clips.Add(clip_asset);
		}

		static List<SwfClipAsset.Sequence> LoadClipSequences(
			SwfAsset asset, SwfSymbolData symbol)
		{
			var sequences = new List<SwfClipAsset.Sequence>();
			if ( IsValidAssetsForFrame(asset, symbol) ) {
				foreach ( var frame in symbol.Frames ) {
					var baked_frame = BakeClipFrame(asset, frame);
					if ( !string.IsNullOrEmpty(frame.Name) &&
						(sequences.Count < 1 || sequences.Last().Name != frame.Name) )
					{
						sequences.Add(new SwfClipAsset.Sequence{Name = frame.Name});
					} else if ( sequences.Count < 1 ) {
						sequences.Add(new SwfClipAsset.Sequence{Name = "Default"});
					}
					sequences.Last().Frames.Add(baked_frame);
				}
			}
			return sequences;
		}

		static bool IsValidAssetsForFrame(
			SwfAsset asset, SwfSymbolData symbol)
		{
			return
				asset && asset.Atlas && asset.Data != null &&
				symbol != null && symbol.Frames != null;
		}

		class BakedGroup {
			public SwfInstanceData.Types Type;
			public int                   ClipDepth;
			public int                   StartVertex;
			public int                   TriangleCount;
			public Material              Material;
		}

		static SwfClipAsset.Frame BakeClipFrame(
			SwfAsset asset, SwfFrameData frame)
		{
			List<uint>       baked_uvs       = new List<uint>();
			List<uint>       baked_mulcolors = new List<uint>();
			List<uint>       baked_addcolors = new List<uint>();
			List<Vector2>    baked_vertices  = new List<Vector2>();
			List<BakedGroup> baked_groups    = new List<BakedGroup>();
			List<Material>   baked_materials = new List<Material>();

			foreach ( var inst in frame.Instances ) {
				var bitmap = inst != null
					? FindBitmapFromAssetData(asset.Data, inst.Bitmap)
					: null;
				if ( bitmap != null && IsVisibleInstance(inst) ) {
					var width  = bitmap.RealSize.x / 20.0f;
					var height = bitmap.RealSize.y / 20.0f;

					var v0 = new Vector2(    0,      0);
					var v1 = new Vector2(width,      0);
					var v2 = new Vector2(width, height);
					var v3 = new Vector2(    0, height);

					var matrix =
						Matrix4x4.Scale(
							new Vector3(1.0f, -1.0f, 1.0f) /
							asset.Settings.PixelsPerUnit) *
						inst.Matrix.ToUMatrix();

					baked_vertices.Add(matrix.MultiplyPoint3x4(v0));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v1));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v2));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v3));

					var source_rect = bitmap.SourceRect;
					baked_uvs.Add(SwfUtils.PackUV(source_rect.xMin, source_rect.yMin));
					baked_uvs.Add(SwfUtils.PackUV(source_rect.xMax, source_rect.yMax));

					uint mul_u0, mul_u1;
					SwfUtils.PackFColorToUInts(
						inst.ColorTrans.Mul, out mul_u0, out mul_u1);
					baked_mulcolors.Add(mul_u0);
					baked_mulcolors.Add(mul_u1);

					uint add_u0, add_u1;
					SwfUtils.PackFColorToUInts(
						inst.ColorTrans.Add, out add_u0, out add_u1);
					baked_addcolors.Add(add_u0);
					baked_addcolors.Add(add_u1);

					if ( baked_groups.Count == 0 ||
						baked_groups[baked_groups.Count - 1].Type      != inst.Type ||
						baked_groups[baked_groups.Count - 1].ClipDepth != inst.ClipDepth )
					{
						baked_groups.Add(new BakedGroup{
							Type          = inst.Type,
							ClipDepth     = inst.ClipDepth,
							StartVertex   = baked_vertices.Count - 4,
							TriangleCount = 0,
							Material      = null
						});
					}

					baked_groups.Last().TriangleCount += 6;
				}
			}

			var settings_holder = SwfSettings.GetHolder();
			for ( var i = 0; i < baked_groups.Count; ++i ) {
				var group = baked_groups[i];
				switch ( group.Type ) {
				case SwfInstanceData.Types.Mask:
					group.Material = settings_holder.GetIncrMaskMaterial();
					break;
				case SwfInstanceData.Types.Group:
					group.Material = settings_holder.GetSimpleMaterial();
					break;
				case SwfInstanceData.Types.Masked:
					group.Material = settings_holder.GetMaskedMaterial(group.ClipDepth);
					break;
				case SwfInstanceData.Types.MaskReset:
					group.Material = settings_holder.GetDecrMaskMaterial();
					break;
				default:
					throw new UnityException(string.Format(
						"SwfAssetPostprocessor. Incorrect instance type: {0}",
						group.Type));
				}
				if ( group.Material ) {
					baked_materials.Add(group.Material);
				} else {
					throw new UnityException(string.Format(
						"SwfAssetPostprocessor. Material for baked group ({0}) not found",
						group.Type));
				}
			}

			var mesh_data = new SwfClipAsset.MeshData{
				SubMeshes = baked_groups
					.Select(p => new SwfClipAsset.SubMeshData{
						StartVertex = p.StartVertex,
						IndexCount  = p.TriangleCount})
					.ToList(),
				Vertices  = baked_vertices,
				UVs       = baked_uvs,
				AddColors = baked_addcolors,
				MulColors = baked_mulcolors};

			return new SwfClipAsset.Frame(
				mesh_data,
				baked_materials.ToArray());
		}

		static SwfBitmapData FindBitmapFromAssetData(SwfAssetData data, int bitmap_id) {
			for ( var i = 0; i < data.Bitmaps.Count; ++i ) {
				var bitmap = data.Bitmaps[i];
				if ( bitmap.Id == bitmap_id ) {
					return bitmap;
				}
			}
			return null;
		}

		static bool IsVisibleInstance(SwfInstanceData inst) {
			var result_color = inst.ColorTrans.ApplyToColor(Color.white);
			return result_color.a >= 0.01f;
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureAssetClips
		//
		// ---------------------------------------------------------------------

		static void ConfigureAssetClips(SwfAsset asset) {
			var clips = GameObject.FindObjectsOfType<SwfClip>();
			foreach ( var clip in clips ) {
				if ( clip && clip.clip && asset.Clips.Contains(clip.clip) ) {
					clip.UpdateAllProperties();
				}
			}
		}
	}
}