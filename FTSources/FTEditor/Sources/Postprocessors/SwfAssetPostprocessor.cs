using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using FTRuntime;
using FTRuntime.Internal;

namespace FTEditor.Postprocessors {
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
				if ( asset.Converting.Stage == 0 ) {
					var new_data = ConfigureBitmaps(
						asset,
						SwfEditorUtils.DecompressAsset<SwfAssetData>(asset.Data));
					asset.Data = SwfEditorUtils.CompressAsset(new_data);
					++asset.Converting.Stage;
					EditorUtility.SetDirty(asset);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
				} else if ( asset.Converting.Stage == 1 ) {
					asset.Atlas = LoadAssetAtlas(asset);
					if ( asset.Atlas ) {
						ConfigureAtlas(asset);
						ConfigureClips(
							asset,
							SwfEditorUtils.DecompressAsset<SwfAssetData>(asset.Data));
					}
					++asset.Converting.Stage;
					EditorUtility.SetDirty(asset);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
				}
			} catch ( Exception e ) {
				Debug.LogErrorFormat(
					"<b>[FlashTools]</b> Postprocess swf asset error: {0}",
					e.Message);
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
			} finally {
				if ( asset ) {
					UpdateAssetClips(asset);
				}
			}
		}

		static Texture2D LoadAssetAtlas(SwfAsset asset) {
			return AssetDatabase.LoadAssetAtPath<Texture2D>(
				GetAtlasPath(asset));
		}

		static string GetAtlasPath(SwfAsset asset) {
			if ( asset.Atlas ) {
				return AssetDatabase.GetAssetPath(asset.Atlas);
			} else {
				var asset_path = AssetDatabase.GetAssetPath(asset);
				return Path.ChangeExtension(asset_path, "._Atlas_.png");
			}
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureBitmaps
		//
		// ---------------------------------------------------------------------

		static SwfAssetData ConfigureBitmaps(SwfAsset asset, SwfAssetData data) {
			var textures = data.Bitmaps
				.Where  (p => p.Redirect == 0)
				.Select (p => new KeyValuePair<ushort, Texture2D>(
					p.Id,
					LoadTextureFromData(p)))
				.ToList();
			var rects = PackAndSaveBitmapsAtlas(
				GetAtlasPath(asset),
				textures.Select(p => p.Value).ToArray(),
				asset.Settings);
			for ( var i = 0; i < data.Bitmaps.Count; ++i ) {
				var bitmap        = data.Bitmaps[i];
				var texture_key   = bitmap.Redirect > 0 ? bitmap.Redirect : bitmap.Id;
				bitmap.SourceRect = SwfRectData.FromURect(
					rects[textures.FindIndex(p => p.Key == texture_key)]);
			}
			return data;
		}

		static Texture2D LoadTextureFromData(SwfBitmapData bitmap) {
			var texture = new Texture2D(
				bitmap.RealWidth, bitmap.RealHeight,
				TextureFormat.ARGB32, false);
			texture.LoadRawTextureData(bitmap.ARGB32);
			return texture;
		}

		struct BitmapsAtlasInfo {
			public Texture2D Atlas;
			public Rect[]    Rects;
		}

		static Rect[] PackAndSaveBitmapsAtlas(
			string atlas_path, Texture2D[] textures, SwfSettingsData settings)
		{
			var atlas_info = PackBitmapsAtlas(textures, settings);
			RevertTexturePremultipliedAlpha(atlas_info.Atlas);
			File.WriteAllBytes(atlas_path, atlas_info.Atlas.EncodeToPNG());
			GameObject.DestroyImmediate(atlas_info.Atlas, true);
			AssetDatabase.ImportAsset(atlas_path);
			return atlas_info.Rects;
		}

		static BitmapsAtlasInfo PackBitmapsAtlas(
			Texture2D[] textures, SwfSettingsData settings)
		{
			var atlas_padding  = Mathf.Max(0,  settings.AtlasPadding);
			var max_atlas_size = Mathf.Max(32, settings.AtlasPowerOfTwo
				? Mathf.ClosestPowerOfTwo(settings.MaxAtlasSize)
				: settings.MaxAtlasSize);
			var atlas = new Texture2D(0, 0);
			var rects = atlas.PackTextures(textures, atlas_padding, max_atlas_size);
			while ( rects == null ) {
				max_atlas_size = Mathf.NextPowerOfTwo(max_atlas_size + 1);
				rects = atlas.PackTextures(textures, atlas_padding, max_atlas_size);
			}
			return settings.AtlasForceSquare && atlas.width != atlas.height
				? BitmapsAtlasToSquare(atlas, rects)
				: new BitmapsAtlasInfo{Atlas = atlas, Rects = rects};
		}

		static BitmapsAtlasInfo BitmapsAtlasToSquare(Texture2D atlas, Rect[] rects) {
			var atlas_size  = Mathf.Max(atlas.width, atlas.height);
			var atlas_scale = new Vector2(atlas.width, atlas.height) / atlas_size;
			var new_atlas   = new Texture2D(atlas_size, atlas_size, TextureFormat.ARGB32, false);
			for ( var i = 0; i < rects.Length; ++i ) {
				var new_position = rects[i].position;
				new_position.Scale(atlas_scale);
				var new_size = rects[i].size;
				new_size.Scale(atlas_scale);
				rects[i] = new Rect(new_position, new_size);
			}
			var fill_pixels = new Color32[atlas_size * atlas_size];
			for ( var i = 0; i < atlas_size * atlas_size; ++i ) {
				fill_pixels[i] = new Color(1,1,1,0);
			}
			new_atlas.SetPixels32(fill_pixels);
			new_atlas.SetPixels32(0, 0, atlas.width, atlas.height, atlas.GetPixels32());
			new_atlas.Apply();
			GameObject.DestroyImmediate(atlas, true);
			return new BitmapsAtlasInfo{
				Atlas = new_atlas,
				Rects = rects};
		}

		static void RevertTexturePremultipliedAlpha(Texture2D texture) {
			var pixels = texture.GetPixels();
			for ( var i = 0; i < pixels.Length; ++i ) {
				var c = pixels[i];
				if ( c.a > 0 ) {
					c.r /= c.a;
					c.g /= c.a;
					c.b /= c.a;
				}
				pixels[i] = c;
			}
			texture.SetPixels(pixels);
			texture.Apply();
		}

		// ---------------------------------------------------------------------
		//
		// ConfigureAtlas
		//
		// ---------------------------------------------------------------------

		static void ConfigureAtlas(SwfAsset asset) {
			var atlas_path                     = AssetDatabase.GetAssetPath(asset.Atlas);
			var atlas_importer                 = GetBitmapsAtlasImporter(asset);
			atlas_importer.spritesheet         = new SpriteMetaData[0];
			atlas_importer.textureType         = TextureImporterType.Sprite;
			atlas_importer.spriteImportMode    = SpriteImportMode.Multiple;
			atlas_importer.spritePixelsPerUnit = asset.Settings.PixelsPerUnit;
			atlas_importer.mipmapEnabled       = asset.Settings.GenerateMipMaps;
			atlas_importer.filterMode          = SwfAtlasFilterToImporterFilter(asset.Settings.AtlasTextureFilter);
			atlas_importer.textureFormat       = SwfAtlasFormatToImporterFormat(asset.Settings.AtlasTextureFormat);
			AssetDatabase.WriteImportSettingsIfDirty(atlas_path);
			AssetDatabase.ImportAsset(atlas_path);
		}

		static TextureImporter GetBitmapsAtlasImporter(SwfAsset asset) {
			var atlas_path     = AssetDatabase.GetAssetPath(asset.Atlas);
			var atlas_importer = AssetImporter.GetAtPath(atlas_path) as TextureImporter;
			if ( !atlas_importer ) {
				throw new UnityException(string.Format(
					"atlas texture importer not found ({0})",
					atlas_path));
			}
			return atlas_importer;
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

		static SwfAssetData ConfigureClips(SwfAsset asset, SwfAssetData data) {
			asset.Clips = asset.Clips.Where(p => !!p).Distinct().ToList();
			foreach ( var symbol in data.Symbols ) {
				ConfigureClip(asset, data, symbol);
			}
			return data;
		}

		static void ConfigureClip(SwfAsset asset, SwfAssetData data, SwfSymbolData symbol) {
			var clip_asset = asset.Clips.FirstOrDefault(p => p.Name == symbol.Name);
			if ( clip_asset ) {
				ConfigureClipAsset(clip_asset, asset, data, symbol);
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip_asset));
			} else {
				var asset_path      = AssetDatabase.GetAssetPath(asset);
				var clip_asset_path = Path.ChangeExtension(asset_path, symbol.Name + ".asset");
				SwfEditorUtils.LoadOrCreateAsset<SwfClipAsset>(clip_asset_path, (new_clip_asset, created) => {
					ConfigureClipAsset(new_clip_asset, asset, data, symbol);
					asset.Clips.Add(new_clip_asset);
				});
			}
		}

		static void ConfigureClipAsset(
			SwfClipAsset clip_asset, SwfAsset asset, SwfAssetData data, SwfSymbolData symbol)
		{
			clip_asset.Name      = symbol.Name;
			clip_asset.Atlas     = asset.Atlas;
			clip_asset.FrameRate = data.FrameRate;
			clip_asset.Sequences = LoadClipSequences(asset, data, symbol);
		}

		static List<SwfClipAsset.Sequence> LoadClipSequences(
			SwfAsset asset, SwfAssetData data, SwfSymbolData symbol)
		{
			var sequences = new List<SwfClipAsset.Sequence>();
			if ( IsValidAssetsForFrame(asset, symbol) ) {
				foreach ( var frame in symbol.Frames ) {
					var baked_frame = BakeClipFrame(asset, data, frame);
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
			public SwfInstanceData.Types  Type;
			public SwfBlendModeData.Types BlendMode;
			public int                    ClipDepth;
			public int                    StartVertex;
			public int                    TriangleCount;
			public Material               Material;
		}

		static SwfClipAsset.Frame BakeClipFrame(
			SwfAsset asset, SwfAssetData data, SwfFrameData frame)
		{
			List<uint>       baked_uvs       = new List<uint>();
			List<uint>       baked_mulcolors = new List<uint>();
			List<uint>       baked_addcolors = new List<uint>();
			List<Vector2>    baked_vertices  = new List<Vector2>();
			List<BakedGroup> baked_groups    = new List<BakedGroup>();
			List<Material>   baked_materials = new List<Material>();

			foreach ( var inst in frame.Instances ) {
				var bitmap = inst != null
					? FindBitmapFromAssetData(data, inst.Bitmap)
					: null;
				if ( bitmap != null && IsVisibleInstance(inst) ) {
					var width  = bitmap.RealWidth  / 20.0f;
					var height = bitmap.RealHeight / 20.0f;

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

					uint mul_pack0, mul_pack1;
					SwfUtils.PackFColorToUInts(
						inst.ColorTrans.mulColor,
						out mul_pack0, out mul_pack1);
					baked_mulcolors.Add(mul_pack0);
					baked_mulcolors.Add(mul_pack1);

					uint add_pack0, add_pack1;
					SwfUtils.PackFColorToUInts(
						inst.ColorTrans.addColor,
						out add_pack0, out add_pack1);
					baked_addcolors.Add(add_pack0);
					baked_addcolors.Add(add_pack1);

					if ( baked_groups.Count == 0 ||
						baked_groups[baked_groups.Count - 1].Type      != inst.Type           ||
						baked_groups[baked_groups.Count - 1].BlendMode != inst.BlendMode.type ||
						baked_groups[baked_groups.Count - 1].ClipDepth != inst.ClipDepth )
					{
						baked_groups.Add(new BakedGroup{
							Type          = inst.Type,
							BlendMode     = inst.BlendMode.type,
							ClipDepth     = inst.ClipDepth,
							StartVertex   = baked_vertices.Count - 4,
							TriangleCount = 0,
							Material      = null
						});
					}

					baked_groups.Last().TriangleCount += 6;
				}
			}

			for ( var i = 0; i < baked_groups.Count; ++i ) {
				var group = baked_groups[i];
				switch ( group.Type ) {
				case SwfInstanceData.Types.Mask:
					group.Material = SwfMaterialCache.GetIncrMaskMaterial();
					break;
				case SwfInstanceData.Types.Group:
					group.Material = SwfMaterialCache.GetSimpleMaterial(group.BlendMode);
					break;
				case SwfInstanceData.Types.Masked:
					group.Material = SwfMaterialCache.GetMaskedMaterial(group.BlendMode, group.ClipDepth);
					break;
				case SwfInstanceData.Types.MaskReset:
					group.Material = SwfMaterialCache.GetDecrMaskMaterial();
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
					.ToArray(),
				Vertices  = baked_vertices .ToArray(),
				UVs       = baked_uvs      .ToArray(),
				AddColors = baked_addcolors.ToArray(),
				MulColors = baked_mulcolors.ToArray()};

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
		// UpdateAssetClips
		//
		// ---------------------------------------------------------------------

		static void UpdateAssetClips(SwfAsset asset) {
			var clips = GameObject.FindObjectsOfType<SwfClip>();
			foreach ( var clip in clips ) {
				if ( clip && clip.clip && asset.Clips.Contains(clip.clip) ) {
					clip.UpdateAllProperties();
				}
			}
		}
	}
}