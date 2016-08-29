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
			string[] imported_assets,
			string[] deleted_assets,
			string[] moved_assets,
			string[] moved_from_asset_paths)
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
				var atlas_asset = LoadAtlasAsset(asset_path);
				if ( atlas_asset != asset.Atlas ) {
					asset.Atlas = atlas_asset;
					ConfigureAtlas(asset_path, asset);
					ConfigureBakedFrames(asset_path, asset);
					EditorUtility.SetDirty(asset);
					AssetDatabase.SaveAssets();
				}
			} catch ( Exception e ) {
				Debug.LogErrorFormat(
					"Postprocess swf animation asset error: {0}",
					e.Message);
				SwfEditorUtils.DeleteAnimationAssetWithDepends(asset);
			}
		}

		static Texture2D LoadAtlasAsset(string asset_path) {
			return AssetDatabase.LoadAssetAtPath<Texture2D>(
				GetAtlasPath(asset_path));
		}

		static string GetAtlasPath(string asset_path) {
			return Path.ChangeExtension(asset_path, ".png");
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureAtlas
		//
		// ---------------------------------------------------------------------

		static void ConfigureAtlas(string asset_path, SwfAnimationAsset asset) {
			var atlas_importer      = GetBitmapsAtlasImporter(asset_path);
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
			AssetDatabase.ImportAsset(
				GetAtlasPath(asset_path),
				ImportAssetOptions.ForceUpdate);
		}

		static TextureImporter GetBitmapsAtlasImporter(string asset_path) {
			var atlas_path     = GetAtlasPath(asset_path);
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
			SwfSettings.AtlasFilter filter)
		{
			switch ( filter ) {
			case SwfSettings.AtlasFilter.Point:
				return FilterMode.Point;
			case SwfSettings.AtlasFilter.Bilinear:
				return FilterMode.Bilinear;
			case SwfSettings.AtlasFilter.Trilinear:
				return FilterMode.Trilinear;
			default:
				throw new UnityException(string.Format(
					"incorrect swf atlas filter ({0})",
					filter));
			}
		}

		static TextureImporterFormat SwfAtlasFormatToImporterFormat(
			SwfSettings.AtlasFormat format)
		{
			switch ( format ) {
			case SwfSettings.AtlasFormat.AutomaticCompressed:
				return TextureImporterFormat.AutomaticCompressed;
			case SwfSettings.AtlasFormat.Automatic16bit:
				return TextureImporterFormat.Automatic16bit;
			case SwfSettings.AtlasFormat.AutomaticTruecolor:
				return TextureImporterFormat.AutomaticTruecolor;
			case SwfSettings.AtlasFormat.AutomaticCrunched:
				return TextureImporterFormat.AutomaticCrunched;
			default:
				throw new UnityException(string.Format(
					"incorrect swf atlas format ({0})",
					format));
			}
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureBakedFrames
		//
		// ---------------------------------------------------------------------

		class BakedGroup {
			public SwfAnimationInstanceType Type;
			public int                      ClipDepth;
			public List<int>                Triangles;
			public Material                 Material;
		}

		static void ConfigureBakedFrames(string asset_path, SwfAnimationAsset asset) {
			RemoveAllSubAssets(asset_path);
			var baked_frames = new List<SwfAnimationAsset.Frame>();
			if ( asset && asset.Atlas && asset.Data != null && asset.Data.Frames.Count > 0 ) {
				for ( var i = 0; i < asset.Data.Frames.Count; ++i ) {
					var frame = asset.Data.Frames[i];
					var baked_frame = BakeFrameFromAnimationFrame(asset, frame);
					baked_frames.Add(baked_frame);
				}
			}
			asset.Frames = baked_frames;
		}

		static void RemoveAllSubAssets(string asset_path) {
			var assets = AssetDatabase.LoadAllAssetsAtPath(asset_path);
			for ( var i = 0; i < assets.Length; ++i ) {
				var asset = assets[i];
				if ( !AssetDatabase.IsMainAsset(asset) ) {
					GameObject.DestroyImmediate(asset, true);
				}
			}
		}

		static SwfAnimationAsset.Frame BakeFrameFromAnimationFrame(
			SwfAnimationAsset asset, SwfAnimationFrameData frame)
		{
			List<Vector2>    baked_uvs       = new List<Vector2>();
			List<Color>      baked_mulcolors = new List<Color>();
			List<Vector4>    baked_addcolors = new List<Vector4>();
			List<Vector3>    baked_vertices  = new List<Vector3>();
			List<BakedGroup> baked_groups    = new List<BakedGroup>();
			List<Material>   baked_materials = new List<Material>();

			for ( var i = 0; i < frame.Instances.Count; ++i ) {
				var inst   = frame.Instances[i];
				var bitmap = inst != null
					? FindBitmapFromAnimationData(asset.Data, inst.Bitmap)
					: null;
				if ( bitmap != null ) {
					var width  = bitmap.RealSize.x / 20.0f;
					var height = bitmap.RealSize.y / 20.0f;

					var v0 = new Vector3(    0,      0, 0);
					var v1 = new Vector3(width,      0, 0);
					var v2 = new Vector3(width, height, 0);
					var v3 = new Vector3(    0, height, 0);

					var frame_offset = Matrix4x4.TRS(
						new Vector2(
							-asset.Data.FrameSize.x * 0.5f / asset.Settings.PixelsPerUnit,
							+asset.Data.FrameSize.y * 0.5f / asset.Settings.PixelsPerUnit),
						Quaternion.identity,
						Vector3.one);

					var matrix =
						frame_offset *
						Matrix4x4.Scale(new Vector3(
							+1.0f / asset.Settings.PixelsPerUnit,
							-1.0f / asset.Settings.PixelsPerUnit,
							+1.0f / asset.Settings.PixelsPerUnit)) *
						inst.Matrix;

					baked_vertices.Add(matrix.MultiplyPoint3x4(v0));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v1));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v2));
					baked_vertices.Add(matrix.MultiplyPoint3x4(v3));

					var source_rect = bitmap.SourceRect;
					baked_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMin));
					baked_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMin));
					baked_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMax));
					baked_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMax));

					baked_mulcolors.Add(inst.ColorTransform.Mul);
					baked_mulcolors.Add(inst.ColorTransform.Mul);
					baked_mulcolors.Add(inst.ColorTransform.Mul);
					baked_mulcolors.Add(inst.ColorTransform.Mul);

					baked_addcolors.Add(inst.ColorTransform.Add);
					baked_addcolors.Add(inst.ColorTransform.Add);
					baked_addcolors.Add(inst.ColorTransform.Add);
					baked_addcolors.Add(inst.ColorTransform.Add);

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

			var default_converter = SwfConverterSettings.GetDefaultConverter();
			for ( var i = 0; i < baked_groups.Count; ++i ) {
				var group = baked_groups[i];
				switch ( group.Type ) {
				case SwfAnimationInstanceType.Mask:
					group.Material = default_converter.GetIncrMaskMaterial();
					break;
				case SwfAnimationInstanceType.Group:
					group.Material = default_converter.GetSimpleMaterial();
					break;
				case SwfAnimationInstanceType.Masked:
					group.Material = default_converter.GetMaskedMaterial(group.ClipDepth);
					break;
				case SwfAnimationInstanceType.MaskReset:
					group.Material = default_converter.GetDecrMaskMaterial();
					break;
				default:
					throw new UnityException(string.Format(
						"SwfAnimationAssetPostprocessor. Incorrect instance type: {0}",
						group.Type));
				}
				if ( group.Material ) {
					group.Material.hideFlags = HideFlags.HideInInspector;
					baked_materials.Add(group.Material);
				} else {
					throw new UnityException(string.Format(
						"SwfAnimationAssetPostprocessor. Material for baked group ({0}) not found",
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

			AssetDatabase.AddObjectToAsset(mesh, asset);
			return new SwfAnimationAsset.Frame{
				Mesh      = mesh,
				Materials = baked_materials.ToArray()};
		}

		static SwfAnimationBitmapData FindBitmapFromAnimationData(SwfAnimationData data, int bitmap_id) {
			for ( var i = 0; i < data.Bitmaps.Count; ++i ) {
				var bitmap = data.Bitmaps[i];
				if ( bitmap.Id == bitmap_id ) {
					return bitmap;
				}
			}
			return null;
		}
	}
}