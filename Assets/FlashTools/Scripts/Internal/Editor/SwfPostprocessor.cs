using UnityEngine;
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
			var swf_animation_data = LoadDataFromSwfFile(swf_asset);
			if ( swf_animation_data != null ) {
				var new_asset_path = Path.ChangeExtension(swf_asset, ".asset");
				var new_asset = AssetDatabase.LoadAssetAtPath<SwfAnimationAsset>(new_asset_path);
				if ( !new_asset ) {
					new_asset = ScriptableObject.CreateInstance<SwfAnimationAsset>();
					AssetDatabase.CreateAsset(new_asset, new_asset_path);
				}
				new_asset.Data = swf_animation_data;
				EditorUtility.SetDirty(new_asset);
				AssetDatabase.SaveAssets();
			}
		}

		static SwfAnimationData LoadDataFromSwfFile(string swf_asset) {
			try {
				var decoder = new SwfDecoder(swf_asset);
				return new SwfAnimationData{
					FrameRate  = decoder.UncompressedHeader.FrameRate,
					Frames     = LoadFramesFromSwfDecoder(decoder)};
			} catch ( Exception e ) {
				Debug.LogErrorFormat("Parsing swf error: {0}", e.Message);
				return null;
			}
		}

		static List<SwfAnimationFrameData> LoadFramesFromSwfDecoder(SwfDecoder decoder) {
			var frames   = new List<SwfAnimationFrameData>();
			var context  = new SwfContext();
			var executer = new SwfContextExecuter(context, 0);
			while ( executer.NextFrame(decoder.Tags, context.DisplayList) ) {
				var frame = new SwfAnimationFrameData();
				frame.Name = context.DisplayList.FrameName;
				AddDisplayListToFrame(
					context,
					context.DisplayList,
					Matrix4x4.identity,
					SwfAnimationColorTransform.identity,
					frame);
				frames.Add(frame);
			}
			return frames;
		}

		static void AddDisplayListToFrame(
			SwfContext ctx, SwfDisplayList dl,
			Matrix4x4 parent_matrix, SwfAnimationColorTransform parent_color_transform,
			SwfAnimationFrameData frame)
		{
			foreach ( var inst in dl.Insts.Values ) {
				switch ( inst.Type ) {
				case SwfDisplayInstType.Shape:
					var shape_def = ctx.Library.FindDefine<SwfLibraryShapeDefine>(inst.Id);
					if ( shape_def != null ) {
						for ( var i = 0; i < shape_def.Bitmaps.Length; ++i ) {
							var bitmap_id     = shape_def.Bitmaps[i];
							var bitmap_matrix = i < shape_def.Matrices.Length
								? shape_def.Matrices[i] : SwfMatrix.identity;
							var bitmap_def = ctx.Library.FindDefine<SwfLibraryBitmapDefine>(bitmap_id);
							if ( bitmap_def != null ) {
								frame.Insts.Add(new SwfAnimationInstData{
									Bitmap         = bitmap_id,
									Matrix         = parent_matrix * inst.Matrix.ToUnityMatrix() * bitmap_matrix.ToUnityMatrix(),
									ColorTransform = parent_color_transform * inst.ColorTransform.ToAnimationColorTransform()});
							}
						}
					}
					break;
				case SwfDisplayInstType.Sprite:
					var sprite_def = ctx.Library.FindDefine<SwfLibrarySpriteDefine>(inst.Id);
					if ( sprite_def != null ) {
						var sprite_inst = inst as SwfDisplaySpriteInst;
						AddDisplayListToFrame(
							ctx,
							sprite_inst.DisplayList,
							parent_matrix * sprite_inst.Matrix.ToUnityMatrix(),
							parent_color_transform * sprite_inst.ColorTransform.ToAnimationColorTransform(),
							frame);
					}
					break;
				default:
					throw new UnityException(string.Format(
						"Unsupported SwfDisplayInstType: {0}", inst.Type));
				}
			}
		}

		/*
		static void SwfBitmapsToAtlas(string swf_asset, List<SwfTagBase> tags) {
			var defines = tags
				.Where(p => p.TagType == SwfTagType.DefineBitsLossless2)
				.Select(p => p as DefineBitsLossless2Tag)
				.Where(p => p.BitmapFormat == 5);
			var textures = new List<Texture2D>();
			foreach ( var define in defines ) {
				var data = Decompress(define.ZlibBitmapData);
				var texture = new Texture2D(
					define.BitmapWidth, define.BitmapHeight,
					TextureFormat.ARGB32, false);
				texture.LoadRawTextureData(data);
				textures.Add(texture);
			}
			var atlas       = new Texture2D(0, 0);
			var atlas_path  = Path.ChangeExtension(swf_asset, ".png");
			atlas.PackTextures(textures.ToArray(), 1, 1024);
			File.WriteAllBytes(atlas_path, atlas.EncodeToPNG());
			GameObject.DestroyImmediate(atlas, true);
			AssetDatabase.ImportAsset(
				atlas_path,
				ImportAssetOptions.ForceUncompressedImport);
		}*/
	}
}
