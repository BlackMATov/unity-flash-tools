using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using SwfTools;
using SwfTools.SwfTags;
using SwfTools.SwfTypes;

namespace FlashTools.Internal {
	public class SwfPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(
			string[] imported_assets,
			string[] deleted_assets,
			string[] moved_assets,
			string[] moved_from_asset_paths)
		{
			var swf_paths = imported_assets
				.Where(p => Path.GetExtension(p).ToLower().Equals(".swf"));
			foreach ( var swf_path in swf_paths ) {
				SwfFileProcess(swf_path);
			}
		}

		static void SwfFileProcess(string swf_path) {
			var swf_asset_path = Path.ChangeExtension(swf_path, ".asset");
			SwfEditorUtils.LoadOrCreateAsset<SwfAsset>(swf_asset_path, swf_asset => {
				SafeLoadSwfAsset(swf_path, swf_asset);
			});
		}

		static void SafeLoadSwfAsset(string swf_path, SwfAsset swf_asset) {
			try {
				var new_data         = LoadSwfAssetData(swf_path);
				swf_asset.Data       = SwfEditorUtils.CompressAsset(new_data);
				swf_asset.Converting = new SwfAsset.ConvertingState();
			} catch ( Exception e ) {
				Debug.LogErrorFormat(
					"<b>[FlashTools]</b> Parsing swf error: {0}",
					e.Message);
			}
		}

		static SwfAssetData LoadSwfAssetData(string swf_path) {
			var library = new SwfLibrary();
			var decoder = new SwfDecoder(swf_path);
			return new SwfAssetData{
				FrameRate = decoder.UncompressedHeader.FrameRate,
				Symbols   = LoadSymbols(library, decoder),
				Bitmaps   = LoadBitmaps(library)};
		}

		// ---------------------------------------------------------------------
		//
		// LoadSymbols
		//
		// ---------------------------------------------------------------------

		static List<SwfSymbolData> LoadSymbols(
			SwfLibrary library, SwfDecoder decoder)
		{
			var symbols = new List<SwfSymbolData>();
			symbols.Add(LoadSymbol("_Stage_", library, decoder.Tags));
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
				symbol_frames.Add(LoadSymbolFrameData(library, disp_lst));
			}
			return new SwfSymbolData{
				Name   = symbol_name,
				Frames = symbol_frames};
		}

		static SwfFrameData LoadSymbolFrameData(
			SwfLibrary library, SwfDisplayList display_list)
		{
			var frame = new SwfFrameData();
			frame.Name = display_list.FrameName;
			return AddDisplayListToFrame(
				library,
				display_list,
				Matrix4x4.identity,
				SwfBlendModeData.identity,
				SwfColorTransData.identity,
				0,
				0,
				null,
				frame);
		}

		static SwfFrameData AddDisplayListToFrame(
			SwfLibrary            library,
			SwfDisplayList        display_list,
			Matrix4x4             parent_matrix,
			SwfBlendModeData      parent_blend_mode,
			SwfColorTransData     parent_color_transform,
			ushort                parent_masked,
			ushort                parent_mask,
			List<SwfInstanceData> parent_masks,
			SwfFrameData          frame)
		{
			var inst_filter_types = display_list.Instances.Values
				.Where(p => p.Visible && p.FilterList.Filters.Count > 0)
				.SelectMany(p => p.FilterList.Filters)
				.Select(p => p.Type)
				.Distinct();
			foreach ( var filter_type in inst_filter_types ) {
				Debug.LogWarningFormat(
					"<b>[FlashTools]</b> SwfSurfaceFilters. Unsupported filter type '{0}'",
					filter_type);
			}
			var self_masks = new List<SwfInstanceData>();
			foreach ( var inst in display_list.Instances.Values.Where(p => p.Visible) ) {
				CheckSelfMasks(self_masks, inst.Depth, frame);
				var child_matrix          = parent_matrix          * inst.Matrix        .ToUMatrix();
				var child_blend_mode      = parent_blend_mode      * inst.BlendMode     .ToBlendModeData();
				var child_color_transform = parent_color_transform * inst.ColorTransform.ToColorTransData();
				switch ( inst.Type ) {
				case SwfDisplayInstanceType.Shape:
					AddShapeInstanceToFrame(
						library,
						inst as SwfDisplayShapeInstance,
						child_matrix,
						child_blend_mode,
						child_color_transform,
						parent_masked,
						parent_mask,
						parent_masks,
						self_masks,
						frame);
					break;
				case SwfDisplayInstanceType.Sprite:
					AddSpriteInstanceToFrame(
						library,
						inst as SwfDisplaySpriteInstance,
						child_matrix,
						child_blend_mode,
						child_color_transform,
						parent_masked,
						parent_mask,
						parent_masks,
						self_masks,
						frame);
					break;
				default:
					throw new UnityException(string.Format(
						"unsupported SwfDisplayInstanceType: {0}", inst.Type));
				}
			}
			CheckSelfMasks(self_masks, ushort.MaxValue, frame);
			return frame;
		}

		static void AddShapeInstanceToFrame(
			SwfLibrary              library,
			SwfDisplayShapeInstance inst,
			Matrix4x4               inst_matrix,
			SwfBlendModeData        inst_blend_mode,
			SwfColorTransData       inst_color_transform,
			ushort                  parent_masked,
			ushort                  parent_mask,
			List<SwfInstanceData>   parent_masks,
			List<SwfInstanceData>   self_masks,
			SwfFrameData            frame)
		{
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
							Matrix     = SwfMatrixData.FromUMatrix(inst_matrix * bitmap_matrix.ToUMatrix()),
							BlendMode  = inst_blend_mode,
							ColorTrans = inst_color_transform});
						if ( parent_mask > 0 ) {
							parent_masks.Add(frame.Instances[frame.Instances.Count - 1]);
						} else if ( inst.ClipDepth > 0 ) {
							self_masks.Add(frame.Instances[frame.Instances.Count - 1]);
						}
					}
				}
			}
		}

		static void AddSpriteInstanceToFrame(
			SwfLibrary               library,
			SwfDisplaySpriteInstance inst,
			Matrix4x4                inst_matrix,
			SwfBlendModeData         inst_blend_mode,
			SwfColorTransData        inst_color_transform,
			ushort                   parent_masked,
			ushort                   parent_mask,
			List<SwfInstanceData>    parent_masks,
			List<SwfInstanceData>    self_masks,
			SwfFrameData             frame)
		{
			var sprite_def = library.FindDefine<SwfLibrarySpriteDefine>(inst.Id);
			if ( sprite_def != null ) {
				AddDisplayListToFrame(
					library,
					inst.DisplayList,
					inst_matrix,
					inst_blend_mode,
					inst_color_transform,
					(ushort)(parent_masked + self_masks.Count),
					(ushort)(parent_mask > 0
						? parent_mask
						: (inst.ClipDepth > 0
							? inst.ClipDepth
							: (ushort)0)),
					parent_mask > 0
						? parent_masks
						: (inst.ClipDepth > 0
							? self_masks
							: null),
					frame);
			}
		}

		static void CheckSelfMasks(
			List<SwfInstanceData> masks,
			ushort                depth,
			SwfFrameData          frame)
		{
			foreach ( var mask in masks ) {
				if ( mask.ClipDepth < depth ) {
					frame.Instances.Add(new SwfInstanceData{
						Type       = SwfInstanceData.Types.MaskReset,
						ClipDepth  = 0,
						Bitmap     = mask.Bitmap,
						Matrix     = mask.Matrix,
						BlendMode  = mask.BlendMode,
						ColorTrans = mask.ColorTrans});
				}
			}
			masks.RemoveAll(p => p.ClipDepth < depth);
		}

		// ---------------------------------------------------------------------
		//
		// LoadBitmaps
		//
		// ---------------------------------------------------------------------

		static List<SwfBitmapData> LoadBitmaps(SwfLibrary library) {
			return library.Defines
				.Where       (p => p.Value.Type == SwfLibraryDefineType.Bitmap)
				.ToDictionary(p => p.Key, p => p.Value as SwfLibraryBitmapDefine)
				.Select      (p => new SwfBitmapData{
					Id         = p.Key,
					ARGB32     = p.Value.ARGB32,
					Redirect   = p.Value.Redirect,
					RealWidth  = p.Value.Width,
					RealHeight = p.Value.Height})
				.ToList();
		}
	}

	// ---------------------------------------------------------------------
	//
	// Extensions
	//
	// ---------------------------------------------------------------------

	static class SwfExtensions {
		public static Matrix4x4 ToUMatrix(this SwfMatrix self) {
			var mat = Matrix4x4.identity;
			mat.m00 = self.ScaleX;
			mat.m10 = self.RotateSkew0;
			mat.m01 = self.RotateSkew1;
			mat.m11 = self.ScaleY;
			mat.m03 = self.TranslateX;
			mat.m13 = self.TranslateY;
			return mat;
		}

		public static SwfBlendModeData ToBlendModeData(this SwfBlendMode self) {
			switch ( self.Value ) {
			case SwfBlendMode.Mode.Normal:
				return new SwfBlendModeData(SwfBlendModeData.Types.Normal);
			case SwfBlendMode.Mode.Layer:
				return new SwfBlendModeData(SwfBlendModeData.Types.Layer);
			case SwfBlendMode.Mode.Multiply:
				return new SwfBlendModeData(SwfBlendModeData.Types.Multiply);
			case SwfBlendMode.Mode.Screen:
				return new SwfBlendModeData(SwfBlendModeData.Types.Screen);
			case SwfBlendMode.Mode.Lighten:
				return new SwfBlendModeData(SwfBlendModeData.Types.Lighten);
			case SwfBlendMode.Mode.Darken:
				return new SwfBlendModeData(SwfBlendModeData.Types.Darken);
			case SwfBlendMode.Mode.Difference:
				return new SwfBlendModeData(SwfBlendModeData.Types.Difference);
			case SwfBlendMode.Mode.Add:
				return new SwfBlendModeData(SwfBlendModeData.Types.Add);
			case SwfBlendMode.Mode.Subtract:
				return new SwfBlendModeData(SwfBlendModeData.Types.Subtract);
			case SwfBlendMode.Mode.Invert:
				return new SwfBlendModeData(SwfBlendModeData.Types.Invert);
			case SwfBlendMode.Mode.Hardlight:
				return new SwfBlendModeData(SwfBlendModeData.Types.Hardlight);
			default:
				Debug.LogWarningFormat(
					"<b>[FlashTools]</b> SwfBlendMode. Unsupported blend mode '{0}'",
					self.Value);
				return new SwfBlendModeData(SwfBlendModeData.Types.Normal);
			}
		}

		public static SwfColorTransData ToColorTransData(this SwfColorTransform self) {
			var trans = SwfColorTransData.identity;
			if ( self.HasAdd ) {
				trans.addColor = new SwfVec4Data(
					self.RAdd / 256.0f,
					self.GAdd / 256.0f,
					self.BAdd / 256.0f,
					self.AAdd / 256.0f);
			}
			if ( self.HasMul ) {
				trans.mulColor = new SwfVec4Data(
					self.RMul / 256.0f,
					self.GMul / 256.0f,
					self.BMul / 256.0f,
					self.AMul / 256.0f);
			}
			return trans;
		}
	}
}