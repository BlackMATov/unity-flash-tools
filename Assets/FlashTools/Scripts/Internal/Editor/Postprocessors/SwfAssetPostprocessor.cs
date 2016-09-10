using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

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
			ConfigureClipSubAssets(clip_asset);
			asset.Clips.Add(clip_asset);
		}

		static void ConfigureClipSubAssets(SwfClipAsset clip_asset) {
			SwfEditorUtils.RemoveAllSubAssets(
				AssetDatabase.GetAssetPath(clip_asset));
			foreach ( var sequence in clip_asset.Sequences ) {
				for ( var i = 0; i < sequence.Frames.Count; ++i ) {
					var mesh  = sequence.Frames[i].Mesh;
					mesh.name = string.Format("{0}_{1}", sequence.Name, i);
					AssetDatabase.AddObjectToAsset(mesh, clip_asset);
				}
			}
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
			public List<int>             Triangles;
			public Material              Material;
		}

		static SwfClipAsset.Frame BakeClipFrame(
			SwfAsset asset, SwfFrameData frame)
		{
			List<Vector2>    baked_uvs       = new List<Vector2>();
			List<Color>      baked_mulcolors = new List<Color>();
			List<Vector4>    baked_addcolors = new List<Vector4>();
			List<Vector3>    baked_vertices  = new List<Vector3>();
			List<BakedGroup> baked_groups    = new List<BakedGroup>();
			List<Material>   baked_materials = new List<Material>();

			foreach ( var inst in frame.Instances ) {
				var bitmap = inst != null
					? FindBitmapFromAssetData(asset.Data, inst.Bitmap)
					: null;
				if ( bitmap != null ) {
					var width  = bitmap.RealSize.x / 20.0f;
					var height = bitmap.RealSize.y / 20.0f;

					var v0 = new Vector3(    0,      0, 0);
					var v1 = new Vector3(width,      0, 0);
					var v2 = new Vector3(width, height, 0);
					var v3 = new Vector3(    0, height, 0);

					var matrix =
						Matrix4x4.Scale(new Vector3(
							+1.0f / asset.Settings.PixelsPerUnit,
							-1.0f / asset.Settings.PixelsPerUnit,
							+1.0f / asset.Settings.PixelsPerUnit)) *
						inst.Matrix.ToUnityMatrix();

					baked_vertices.Add(matrix.MultiplyPoint3x4(v0));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v1));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v2));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v3));

					var source_rect = bitmap.SourceRect;
					baked_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMin));
					baked_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMin));
					baked_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMax));
					baked_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMax));

					baked_mulcolors.Add(inst.ColorTrans.Mul);
					baked_mulcolors.Add(inst.ColorTrans.Mul);
					baked_mulcolors.Add(inst.ColorTrans.Mul);
					baked_mulcolors.Add(inst.ColorTrans.Mul);

					baked_addcolors.Add(inst.ColorTrans.Add);
					baked_addcolors.Add(inst.ColorTrans.Add);
					baked_addcolors.Add(inst.ColorTrans.Add);
					baked_addcolors.Add(inst.ColorTrans.Add);

					if ( baked_groups.Count == 0 ||
						baked_groups[baked_groups.Count - 1].Type      != inst.Type ||
						baked_groups[baked_groups.Count - 1].ClipDepth != inst.ClipDepth )
					{
						baked_groups.Add(new BakedGroup{
							Type      = inst.Type,
							ClipDepth = inst.ClipDepth,
							Triangles = new List<int>(),
							Material  = null
						});
					}

					baked_groups.Last().Triangles.Add(baked_vertices.Count - 4 + 2);
					baked_groups.Last().Triangles.Add(baked_vertices.Count - 4 + 1);
					baked_groups.Last().Triangles.Add(baked_vertices.Count - 4 + 0);
					baked_groups.Last().Triangles.Add(baked_vertices.Count - 4 + 0);
					baked_groups.Last().Triangles.Add(baked_vertices.Count - 4 + 3);
					baked_groups.Last().Triangles.Add(baked_vertices.Count - 4 + 2);
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
					group.Material.hideFlags = HideFlags.HideInInspector;
					baked_materials.Add(group.Material);
				} else {
					throw new UnityException(string.Format(
						"SwfAssetPostprocessor. Material for baked group ({0}) not found",
						group.Type));
				}
			}

			var mesh = new Mesh();
			mesh.subMeshCount = baked_groups.Count;
			mesh.SetVertices(baked_vertices);
			for ( var i = 0; i < baked_groups.Count; ++i ) {
				mesh.SetTriangles(baked_groups[i].Triangles, i);
			}
			mesh.SetUVs(0, baked_uvs);
			mesh.SetUVs(1, baked_addcolors);
			mesh.SetColors(baked_mulcolors);
			mesh.RecalculateNormals();
			mesh.Optimize();

			return new SwfClipAsset.Frame{
				Mesh      = mesh,
				Materials = baked_materials.ToArray()};
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