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
			var new_asset_path = Path.ChangeExtension(swf_asset, ".asset");
			var new_asset = AssetDatabase.LoadAssetAtPath<SwfAnimationAsset>(new_asset_path);
			if ( !new_asset ) {
				new_asset = ScriptableObject.CreateInstance<SwfAnimationAsset>();
				AssetDatabase.CreateAsset(new_asset, new_asset_path);
			}
			if ( LoadDataFromSwfFile(swf_asset, new_asset) ) {
				EditorUtility.SetDirty(new_asset);
				AssetDatabase.SaveAssets();
			} else {
				AssetDatabase.DeleteAsset(new_asset_path);
			}
		}

		static bool LoadDataFromSwfFile(string swf_asset, SwfAnimationAsset asset) {
			try {
				asset.Data = LoadAnimationDataFromSwfDecoder(
					swf_asset,
					asset,
					new SwfDecoder(swf_asset));
				return true;
			} catch ( Exception e ) {
				Debug.LogErrorFormat("Parsing swf error: {0}", e.Message);
				return false;
			}
		}

		static SwfAnimationData LoadAnimationDataFromSwfDecoder(
			string swf_asset, SwfAnimationAsset asset, SwfDecoder decoder)
		{
			var animation_data = new SwfAnimationData{
				FrameRate = decoder.UncompressedHeader.FrameRate
			};
			var context  = new SwfContext();
			var executer = new SwfContextExecuter(context, 0);
			while ( executer.NextFrame(decoder.Tags, context.DisplayList) ) {
				animation_data.Frames.Add(
					LoadAnimationFrameFromContext(context));
			}
			animation_data.Bitmaps = LoadBitmapsFromContext(swf_asset, asset, context);
			return animation_data;
		}

		static SwfAnimationFrameData LoadAnimationFrameFromContext(SwfContext context) {
			var frame = new SwfAnimationFrameData();
			frame.Name = context.DisplayList.FrameName;
			return AddDisplayListToFrame(
				context,
				context.DisplayList,
				0,
				0,
				null,
				Matrix4x4.identity,
				SwfAnimationColorTransform.identity,
				frame);
		}

		static SwfAnimationFrameData AddDisplayListToFrame(
			SwfContext                     ctx,
			SwfDisplayList                 dl,
			ushort                         parent_masked,
			ushort                         parent_mask,
			List<SwfAnimationInstanceData> parent_masks,
			Matrix4x4                      parent_matrix,
			SwfAnimationColorTransform     parent_color_transform,
			SwfAnimationFrameData          frame)
		{
			var self_masks = new List<SwfAnimationInstanceData>();
			foreach ( var inst in dl.Instances.Values.Where(p => p.Visible) ) {
				CheckSelfMasks(self_masks, inst.Depth, frame);
				var child_matrix          = parent_matrix * inst.Matrix.ToUnityMatrix();
				var child_color_transform = parent_color_transform * inst.ColorTransform.ToAnimationColorTransform();
				switch ( inst.Type ) {
				case SwfDisplayInstanceType.Shape:
					var shape_def = ctx.Library.FindDefine<SwfLibraryShapeDefine>(inst.Id);
					if ( shape_def != null ) {
						for ( var i = 0; i < shape_def.Bitmaps.Length; ++i ) {
							var bitmap_id     = shape_def.Bitmaps[i];
							var bitmap_matrix = i < shape_def.Matrices.Length ? shape_def.Matrices[i] : SwfMatrix.identity;
							var bitmap_def    = ctx.Library.FindDefine<SwfLibraryBitmapDefine>(bitmap_id);
							if ( bitmap_def != null ) {
								var frame_inst_type =
									(parent_mask > 0 || inst.ClipDepth > 0)
										? SwfAnimationInstanceType.Mask
										: (parent_masked > 0 || self_masks.Count > 0)
											? SwfAnimationInstanceType.Masked
											: SwfAnimationInstanceType.Group;
								var frame_inst_clip_depth =
									(parent_mask > 0)
										? parent_mask
										: (inst.ClipDepth > 0)
											? inst.ClipDepth
											: parent_masked + self_masks.Count;
								frame.Instances.Add(new SwfAnimationInstanceData{
									Type           = frame_inst_type,
									ClipDepth      = (ushort)frame_inst_clip_depth,
									Bitmap         = bitmap_id,
									Matrix         = child_matrix * bitmap_matrix.ToUnityMatrix(),
									ColorTransform = child_color_transform});
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
					var sprite_def = ctx.Library.FindDefine<SwfLibrarySpriteDefine>(inst.Id);
					if ( sprite_def != null ) {
						var sprite_inst = inst as SwfDisplaySpriteInstance;
						AddDisplayListToFrame(
							ctx,
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
			List<SwfAnimationInstanceData> masks, ushort depth, SwfAnimationFrameData frame)
		{
			foreach ( var mask in masks ) {
				if ( mask.ClipDepth < depth ) {
					frame.Instances.Add(new SwfAnimationInstanceData{
						Type           = SwfAnimationInstanceType.MaskReset,
						ClipDepth      = 0,
						Bitmap         = mask.Bitmap,
						Matrix         = mask.Matrix,
						ColorTransform = mask.ColorTransform
					});
				}
			}
			masks.RemoveAll(p => p.ClipDepth < depth);
		}

		static List<SwfAnimationBitmapData> LoadBitmapsFromContext(
			string swf_asset, SwfAnimationAsset asset, SwfContext context)
		{
			var bitmap_defines = context.Library.Defines
				.Where  (p => p.Value.Type == SwfLibraryDefineType.Bitmap)
				.Select (p => new KeyValuePair<int, SwfLibraryBitmapDefine>(p.Key, p.Value as SwfLibraryBitmapDefine))
				.ToArray();

			var textures = bitmap_defines
				.Select(p => LoadTextureFromBitmapDefine(p.Value));

			var atlas = new Texture2D(0, 0, TextureFormat.ARGB32, false);
			var atlas_rects = atlas.PackTextures(
				textures.ToArray(), asset.AtlasPadding, asset.MaxAtlasSize);

			File.WriteAllBytes(
				GetAtlasPath(swf_asset),
				atlas.EncodeToPNG());
			GameObject.DestroyImmediate(atlas, true);
			AssetDatabase.ImportAsset(
				GetAtlasPath(swf_asset),
				ImportAssetOptions.ForceUncompressedImport);

			var bitmaps = new List<SwfAnimationBitmapData>();
			for ( var i = 0; i < bitmap_defines.Length; ++i ) {
				var bitmap_define = bitmap_defines[i];
				var bitmap_data = new SwfAnimationBitmapData{
					Id         = bitmap_define.Key,
					RealSize   = new Vector2(bitmap_define.Value.Width, bitmap_define.Value.Height),
					SourceRect = atlas_rects[i]
				};
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
			for (int y = 0; y < texture.height; ++y) {
				for (int x = 0; x < texture.width; ++x) {
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

		static string GetAtlasPath(string swf_asset) {
			return Path.ChangeExtension(swf_asset, ".png");
		}
	}
}