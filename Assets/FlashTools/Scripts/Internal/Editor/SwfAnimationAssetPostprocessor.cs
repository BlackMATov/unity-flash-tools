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
			}
		}

		static Texture2D LoadAtlasAsset(string asset_path) {
			return AssetDatabase.LoadAssetAtPath<Texture2D>(
				GetAtlasPath(asset_path));
		}

		static string GetAtlasPath(string asset_path) {
			return Path.ChangeExtension(asset_path, ".png");
		}

		// ------------------------------------------------------------------------
		//
		// ConfigureAtlas
		//
		// ------------------------------------------------------------------------


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

		// ------------------------------------------------------------------------
		//
		// ConfigureBakedFrames
		//
		// ------------------------------------------------------------------------

		class BakeGroup {
			public SwfAnimationInstanceType Type;
			public int                      ClipDepth;
			public List<int>                Triangles;
			public Material                 Material;
		}

		static void ConfigureBakedFrames(string asset_path, SwfAnimationAsset asset) {
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

		static SwfAnimationAsset.Frame BakeFrameFromAnimationFrame(SwfAnimationAsset asset, SwfAnimationFrameData frame) {
			List<Vector2>   _uvs       = new List<Vector2>();
			List<Color>     _mulcolors = new List<Color>();
			List<Vector4>   _addcolors = new List<Vector4>();
			List<Vector3>   _vertices  = new List<Vector3>();
			List<BakeGroup> _groups    = new List<BakeGroup>();
			List<Material>  _materials = new List<Material>();

			for ( var i = 0; i < frame.Instances.Count; ++i ) {
				var inst   = frame.Instances[i];
				var bitmap = inst != null ? FindBitmapFromAnimationData(asset.Data, inst.Bitmap) : null;
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
							+1.0f / asset.Settings.PixelsPerUnit)) * inst.Matrix;

					_vertices.Add(matrix.MultiplyPoint3x4(v0));
					_vertices.Add(matrix.MultiplyPoint3x4(v1));
					_vertices.Add(matrix.MultiplyPoint3x4(v2));
					_vertices.Add(matrix.MultiplyPoint3x4(v3));

					var source_rect = bitmap.SourceRect;
					_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMin));
					_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMin));
					_uvs.Add(new Vector2(source_rect.xMax, source_rect.yMax));
					_uvs.Add(new Vector2(source_rect.xMin, source_rect.yMax));

					_mulcolors.Add(inst.ColorTransform.Mul);
					_mulcolors.Add(inst.ColorTransform.Mul);
					_mulcolors.Add(inst.ColorTransform.Mul);
					_mulcolors.Add(inst.ColorTransform.Mul);

					_addcolors.Add(inst.ColorTransform.Add);
					_addcolors.Add(inst.ColorTransform.Add);
					_addcolors.Add(inst.ColorTransform.Add);
					_addcolors.Add(inst.ColorTransform.Add);

					if ( _groups.Count == 0 ||
						_groups[_groups.Count - 1].Type != inst.Type ||
						_groups[_groups.Count - 1].ClipDepth != inst.ClipDepth )
					{
						_groups.Add(new BakeGroup{
							Type      = inst.Type,
							ClipDepth = inst.ClipDepth,
							Triangles = new List<int>(),
							Material  = null
						});
					}

					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 2);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 1);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 0);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 0);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 3);
					_groups[_groups.Count - 1].Triangles.Add(_vertices.Count - 4 + 2);
				}
			}

			var default_converter = SwfConverterSettings.GetDefaultConverter();
			for ( var i = 0; i < _groups.Count; ++i ) {
				var group = _groups[i];
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
				}
			}

			for ( var i = 0; i < _groups.Count; ++i ) {
				var group = _groups[i];
				_materials.Add(group.Material);
			}

			var mesh = new Mesh();
			mesh.subMeshCount = _groups.Count;
			mesh.SetVertices(_vertices);
			for ( var i = 0; i < _groups.Count; ++i ) {
				mesh.SetTriangles(_groups[i].Triangles, i);
			}
			mesh.SetUVs(0, _uvs);
			mesh.SetUVs(1, _addcolors);
			mesh.SetColors(_mulcolors);
			mesh.RecalculateNormals();

			AssetDatabase.AddObjectToAsset(mesh, asset);

			return new SwfAnimationAsset.Frame{
				Mesh      = mesh,
				Materials = _materials.ToArray()};
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