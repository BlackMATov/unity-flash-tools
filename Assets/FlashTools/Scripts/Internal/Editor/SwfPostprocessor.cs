﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using FlashTools.Internal.SwfTools;
using FlashTools.Internal.SwfTools.SwfTags;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal {
	public class SwfPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(
			string[] imported_assets,
			string[] deleted_assets,
			string[] moved_assets,
			string[] moved_from_asset_paths)
		{
			var swf_asset_paths = imported_assets
				.Where(p => Path.GetExtension(p).ToLower().Equals(".swf"));
			foreach ( var swf_asset_path in swf_asset_paths ) {
				SwfAssetProcess(swf_asset_path);
			}
		}

		static void SwfAssetProcess(string swf_asset) {
			var new_asset_path = SwfEditorUtils.GetSettingsPathFromSwfPath(swf_asset);
			var new_asset = AssetDatabase.LoadAssetAtPath<SwfAsset>(new_asset_path);
			if ( !new_asset ) {
				new_asset = ScriptableObject.CreateInstance<SwfAsset>();
				AssetDatabase.CreateAsset(new_asset, new_asset_path);
			}
			if ( LoadDataFromSwfFile(swf_asset, new_asset) ) {
				EditorUtility.SetDirty(new_asset);
				AssetDatabase.SaveAssets();
			} else {
				SwfEditorUtils.DeleteAssetWithDepends(new_asset);
			}
		}

		static bool LoadDataFromSwfFile(string swf_asset, SwfAsset asset) {
			try {
				if ( asset.Atlas ) {
					AssetDatabase.DeleteAsset(
						AssetDatabase.GetAssetPath(asset.Atlas));
					asset.Atlas = null;
				}
				asset.Data = LoadDataFromSwfDecoder(
					swf_asset,
					asset,
					new SwfDecoder(swf_asset));
				return true;
			} catch ( Exception e ) {
				Debug.LogErrorFormat("Parsing swf error: {0}", e.Message);
				return false;
			}
		}

		static SwfAssetData LoadDataFromSwfDecoder(
			string swf_asset, SwfAsset asset, SwfDecoder decoder)
		{
			var library = new SwfLibrary();
			return new SwfAssetData{
				FrameRate = decoder.UncompressedHeader.FrameRate,
				Symbols   = LoadSymbols(library, decoder),
				Bitmaps   = LoadBitmaps(swf_asset, asset.Settings, library)};
		}

		static List<SwfSymbolData> LoadSymbols(
			SwfLibrary library, SwfDecoder decoder)
		{
			var symbols = new List<SwfSymbolData>();
			symbols.Add(LoadSymbol("_Stage", library, decoder.Tags));
			var sprite_defs = library.Defines.Values
				.OfType<SwfLibrarySpriteDefine>()
				.Where(p => !string.IsNullOrEmpty(p.ExportName));
			foreach ( var sprite_def in sprite_defs ) {
				var name = sprite_def.ExportName;
				var tags = sprite_def.ControlTags.Tags;
				symbols.Add(LoadSymbol(name, library, tags));
			}
			return symbols;
		}

		static SwfSymbolData LoadSymbol(
			string symbol_name, SwfLibrary library, List<SwfTagBase> tags)
		{
			var disp_lst      = new SwfDisplayList();
			var executer      = new SwfContextExecuter(library, 0);
			var symbol_frames = new List<SwfFrameData>();
			while ( executer.NextFrame(tags, disp_lst) ) {
				symbol_frames.Add(LoadSymbolFrame(library, disp_lst));
			}
			return new SwfSymbolData{
				Name   = symbol_name,
				Frames = symbol_frames};
		}

		static SwfFrameData LoadSymbolFrame(
			SwfLibrary library, SwfDisplayList display_list)
		{
			var frame = new SwfFrameData();
			frame.Name = display_list.FrameName;
			return AddDisplayListToFrame(
				library,
				display_list,
				0,
				0,
				null,
				Matrix4x4.identity,
				SwfColorTransData.identity,
				frame);
		}

		static SwfFrameData AddDisplayListToFrame(
			SwfLibrary            library,
			SwfDisplayList        display_list,
			ushort                parent_masked,
			ushort                parent_mask,
			List<SwfInstanceData> parent_masks,
			Matrix4x4             parent_matrix,
			SwfColorTransData     parent_color_transform,
			SwfFrameData          frame)
		{
			var self_masks = new List<SwfInstanceData>();
			foreach ( var inst in display_list.Instances.Values.Where(p => p.Visible) ) {
				CheckSelfMasks(self_masks, inst.Depth, frame);
				var child_matrix          = parent_matrix * inst.Matrix.ToUnityMatrix();
				var child_color_transform = parent_color_transform * inst.ColorTransform.ToColorTransData();
				switch ( inst.Type ) {
				case SwfDisplayInstanceType.Shape:
					var shape_def = library.FindDefine<SwfLibraryShapeDefine>(inst.Id);
					if ( shape_def != null ) {
						for ( var i = 0; i < shape_def.Bitmaps.Length; ++i ) {
							var bitmap_id     = shape_def.Bitmaps[i];
							var bitmap_matrix = i < shape_def.Matrices.Length ? shape_def.Matrices[i] : SwfMatrix.identity;
							var bitmap_def    = library.FindDefine<SwfLibraryBitmapDefine>(bitmap_id);
							if ( bitmap_def != null ) {
								var frame_inst_type =
									(parent_mask > 0 || inst.ClipDepth > 0)
										? SwfInstanceData.Types.Mask
										: (parent_masked > 0 || self_masks.Count > 0)
											? SwfInstanceData.Types.Masked
											: SwfInstanceData.Types.Group;
								var frame_inst_clip_depth =
									(parent_mask > 0)
										? parent_mask
										: (inst.ClipDepth > 0)
											? inst.ClipDepth
											: parent_masked + self_masks.Count;
								frame.Instances.Add(new SwfInstanceData{
									Type       = frame_inst_type,
									ClipDepth  = (ushort)frame_inst_clip_depth,
									Bitmap     = bitmap_id,
									Matrix     = child_matrix * bitmap_matrix.ToUnityMatrix(),
									ColorTrans = child_color_transform});
								if ( parent_mask > 0 ) {
									parent_masks.Add(frame.Instances[frame.Instances.Count - 1]);
								} else if ( inst.ClipDepth > 0 ) {
									self_masks.Add(frame.Instances[frame.Instances.Count - 1]);
								}
							}
						}
					}
					break;
				case SwfDisplayInstanceType.Sprite:
					var sprite_def = library.FindDefine<SwfLibrarySpriteDefine>(inst.Id);
					if ( sprite_def != null ) {
						var sprite_inst = inst as SwfDisplaySpriteInstance;
						AddDisplayListToFrame(
							library,
							sprite_inst.DisplayList,
							(ushort)(parent_masked + self_masks.Count),
							(ushort)(parent_mask > 0 ? parent_mask : (inst.ClipDepth > 0 ? inst.ClipDepth : (ushort)0)),
							parent_mask > 0 ? parent_masks : (inst.ClipDepth > 0 ? self_masks : null),
							child_matrix,
							child_color_transform,
							frame);
					}
					break;
				default:
					throw new UnityException(string.Format(
						"Unsupported SwfDisplayInstanceType: {0}", inst.Type));
				}
			}
			CheckSelfMasks(self_masks, ushort.MaxValue, frame);
			return frame;
		}

		static void CheckSelfMasks(
			List<SwfInstanceData> masks, ushort depth, SwfFrameData frame)
		{
			foreach ( var mask in masks ) {
				if ( mask.ClipDepth < depth ) {
					frame.Instances.Add(new SwfInstanceData{
						Type       = SwfInstanceData.Types.MaskReset,
						ClipDepth  = 0,
						Bitmap     = mask.Bitmap,
						Matrix     = mask.Matrix,
						ColorTrans = mask.ColorTrans});
				}
			}
			masks.RemoveAll(p => p.ClipDepth < depth);
		}

		static List<SwfBitmapData> LoadBitmaps(
			string swf_asset, SwfSettingsData settings, SwfLibrary library)
		{
			var bitmap_defines = library.Defines
				.Where  (p => p.Value.Type == SwfLibraryDefineType.Bitmap)
				.Select (p => new KeyValuePair<int, SwfLibraryBitmapDefine>(
					p.Key, p.Value as SwfLibraryBitmapDefine))
				.ToArray();
			var textures = bitmap_defines
				.Select (p => LoadTextureFromBitmapDefine(p.Value))
				.ToArray();
			var rects = PackAndSaveBitmapsAtlas(
				swf_asset, textures, settings);
			var bitmaps = new List<SwfBitmapData>(bitmap_defines.Length);
			for ( var i = 0; i < bitmap_defines.Length; ++i ) {
				var bitmap_define = bitmap_defines[i];
				var bitmap_data = new SwfBitmapData{
					Id         = bitmap_define.Key,
					RealSize   = new Vector2(bitmap_define.Value.Width, bitmap_define.Value.Height),
					SourceRect = rects[i]};
				bitmaps.Add(bitmap_data);
			}
			return bitmaps;
		}

		static Texture2D LoadTextureFromBitmapDefine(SwfLibraryBitmapDefine bitmap) {
			var texture = new Texture2D(
				bitmap.Width, bitmap.Height,
				TextureFormat.ARGB32, false);
			texture.LoadRawTextureData(bitmap.ARGB32);
			RevertTexturePremultipliedAlpha(texture);
			return texture;
		}

		static void RevertTexturePremultipliedAlpha(Texture2D texture) {
			for ( int y = 0; y < texture.height; ++y ) {
				for ( int x = 0; x < texture.width; ++x ) {
					var c = texture.GetPixel(x, y);
					if ( c.a > 0 ) {
						c.r /= c.a;
						c.g /= c.a;
						c.b /= c.a;
					}
					texture.SetPixel(x, y, c);
				}
			}
			texture.Apply();
		}

		struct BitmapsAtlasInfo {
			public Texture2D Atlas;
			public Rect[]    Rects;
		}

		static Rect[] PackAndSaveBitmapsAtlas(
			string swf_asset, Texture2D[] textures, SwfSettingsData settings)
		{
			var atlas_info = PackBitmapsAtlas(textures, settings);
			var atlas_path = SwfEditorUtils.GetAtlasPathFromSwfPath(swf_asset);
			File.WriteAllBytes(atlas_path, atlas_info.Atlas.EncodeToPNG());
			GameObject.DestroyImmediate(atlas_info.Atlas, true);
			AssetDatabase.ImportAsset(atlas_path);
			return atlas_info.Rects;
		}

		static BitmapsAtlasInfo PackBitmapsAtlas(Texture2D[] textures, SwfSettingsData settings) {
			var atlas_padding  = Mathf.Max(0,  settings.AtlasPadding);
			var max_atlas_size = Mathf.Max(32, settings.AtlasPowerOfTwo
				? Mathf.ClosestPowerOfTwo(settings.MaxAtlasSize)
				: settings.MaxAtlasSize);
			var atlas = new Texture2D(0, 0);
			var rects = atlas.PackTextures(textures, atlas_padding, max_atlas_size);
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
			return new BitmapsAtlasInfo{Atlas = new_atlas, Rects = rects};
		}
	}
}