using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTags;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools {
	public class SwfContextExecuter : SwfTagVisitor<SwfDisplayList, SwfDisplayList> {
		public SwfContext MainContex = null;
		public int        CurrentTag = 0;

		public SwfContextExecuter(SwfContext main_context, int current_tag) {
			MainContex = main_context;
			CurrentTag = current_tag;
		}

		public bool NextFrame(List<SwfTagBase> tags, SwfDisplayList dl) {
			dl.FrameName = string.Empty;
			while ( CurrentTag < tags.Count ) {
				var tag = tags[CurrentTag++];
				TagToDebugLog(tag);
				tag.AcceptVistor(this, dl);
				if ( tag.TagType == SwfTagType.ShowFrame ) {
					ChildrenNextFrameLooped(dl);
					return true;
				}
			}
			ChildrenNextFrameLooped(dl);
			return false;
		}

		public SwfDisplayList Visit(PlaceObjectTag tag, SwfDisplayList dl) {
			var is_shape  =
				MainContex.Library.HasDefine<SwfLibraryShapeDefine >(tag.CharacterId);
			var is_sprite =
				MainContex.Library.HasDefine<SwfLibrarySpriteDefine>(tag.CharacterId);
			SwfDisplayInstance new_inst = null;
			if ( is_shape ) {
				new_inst = new SwfDisplayShapeInstance();
			} else if ( is_sprite ) {
				new_inst = new SwfDisplaySpriteInstance();
			}
			if ( new_inst != null ) {
				new_inst.Id             = tag.CharacterId;
				new_inst.Depth          = tag.Depth;
				new_inst.ClipDepth      = 0;
				new_inst.Visible        = true;
				new_inst.Matrix         = tag.Matrix;
				new_inst.ColorTransform = tag.ColorTransform;
				dl.Instances.Add(new_inst.Depth, new_inst);
			}
			return dl;
		}

		public SwfDisplayList Visit(PlaceObject2Tag tag, SwfDisplayList dl) {
			var is_shape  = tag.HasCharacter
				? MainContex.Library.HasDefine<SwfLibraryShapeDefine >(tag.CharacterId)
				: false;
			var is_sprite = tag.HasCharacter
				? MainContex.Library.HasDefine<SwfLibrarySpriteDefine>(tag.CharacterId)
				: false;
			if ( tag.HasCharacter ) {
				SwfDisplayInstance old_inst = null;
				if ( tag.Move ) { // replace character
					if ( dl.Instances.TryGetValue(tag.Depth, out old_inst) ) {
						dl.Instances.Remove(tag.Depth);
					}
				}
				// new character
				SwfDisplayInstance new_inst = null;
				if ( is_shape ) {
					new_inst = new SwfDisplayShapeInstance();
				} else if ( is_sprite ) {
					new_inst = new SwfDisplaySpriteInstance();
				}
				if ( new_inst != null ) {
					new_inst.Id             = tag.CharacterId;
					new_inst.Depth          = tag.Depth;
					new_inst.ClipDepth      = tag.HasClipDepth      ? tag.ClipDepth      : (old_inst != null ? old_inst.ClipDepth      : (ushort)0);
					new_inst.Visible        = true;
					new_inst.Matrix         = tag.HasMatrix         ? tag.Matrix         : (old_inst != null ? old_inst.Matrix         : SwfMatrix.identity);
					new_inst.ColorTransform = tag.HasColorTransform ? tag.ColorTransform : (old_inst != null ? old_inst.ColorTransform : SwfColorTransform.identity);
					dl.Instances.Add(new_inst.Depth, new_inst);
				}
			} else if ( tag.Move ) { // move character
				SwfDisplayInstance inst;
				if ( dl.Instances.TryGetValue(tag.Depth, out inst) ) {
					if ( tag.HasClipDepth ) {
						inst.ClipDepth = tag.ClipDepth;
					}
					if ( tag.HasMatrix ) {
						inst.Matrix = tag.Matrix;
					}
					if ( tag.HasColorTransform ) {
						inst.ColorTransform = tag.ColorTransform;
					}
				}
			}
			return dl;
		}

		public SwfDisplayList Visit(PlaceObject3Tag tag, SwfDisplayList dl) {
			var is_shape  = tag.HasCharacter
				? MainContex.Library.HasDefine<SwfLibraryShapeDefine >(tag.CharacterId)
				: false;
			var is_sprite = tag.HasCharacter
				? MainContex.Library.HasDefine<SwfLibrarySpriteDefine>(tag.CharacterId)
				: false;
			if ( tag.HasCharacter ) {
				SwfDisplayInstance old_inst = null;
				if ( tag.Move ) { // replace character
					if ( dl.Instances.TryGetValue(tag.Depth, out old_inst) ) {
						dl.Instances.Remove(tag.Depth);
					}
				}
				// new character
				SwfDisplayInstance new_inst = null;
				if ( is_shape ) {
					new_inst = new SwfDisplayShapeInstance();
				} else if ( is_sprite ) {
					new_inst = new SwfDisplaySpriteInstance();
				}
				if ( new_inst != null ) {
					new_inst.Id             = tag.CharacterId;
					new_inst.Depth          = tag.Depth;
					new_inst.ClipDepth      = tag.HasClipDepth      ? tag.ClipDepth      : (old_inst != null ? old_inst.ClipDepth      : (ushort)0);
					new_inst.Visible        = tag.HasVisible        ? tag.Visible        : (old_inst != null ? old_inst.Visible        : true);
					new_inst.Matrix         = tag.HasMatrix         ? tag.Matrix         : (old_inst != null ? old_inst.Matrix         : SwfMatrix.identity);
					new_inst.ColorTransform = tag.HasColorTransform ? tag.ColorTransform : (old_inst != null ? old_inst.ColorTransform : SwfColorTransform.identity);
					dl.Instances.Add(new_inst.Depth, new_inst);
				}
			} else if ( tag.Move ) { // move character
				SwfDisplayInstance inst;
				if ( dl.Instances.TryGetValue(tag.Depth, out inst) ) {
					if ( tag.HasClipDepth ) {
						inst.ClipDepth = tag.ClipDepth;
					}
					if ( tag.HasVisible ) {
						inst.Visible = tag.Visible;
					}
					if ( tag.HasMatrix ) {
						inst.Matrix = tag.Matrix;
					}
					if ( tag.HasColorTransform ) {
						inst.ColorTransform = tag.ColorTransform;
					}
				}
			}
			return dl;
		}

		public SwfDisplayList Visit(RemoveObjectTag tag, SwfDisplayList dl) {
			dl.Instances.Remove(tag.Depth);
			return dl;
		}

		public SwfDisplayList Visit(RemoveObject2Tag tag, SwfDisplayList dl) {
			dl.Instances.Remove(tag.Depth);
			return dl;
		}

		public SwfDisplayList Visit(ShowFrameTag tag, SwfDisplayList dl) {
			return dl;
		}

		public SwfDisplayList Visit(SetBackgroundColorTag tag, SwfDisplayList dl) {
			return dl;
		}

		public SwfDisplayList Visit(FrameLabelTag tag, SwfDisplayList dl) {
			dl.FrameName = tag.Name;
			return dl;
		}

		public SwfDisplayList Visit(EndTag tag, SwfDisplayList dl) {
			return dl;
		}

		public SwfDisplayList Visit(DefineSceneAndFrameLabelDataTag tag, SwfDisplayList dl) {
			return dl;
		}

		public SwfDisplayList Visit(DefineShapeTag tag, SwfDisplayList dl) {
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineShape2Tag tag, SwfDisplayList dl) {
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineShape3Tag tag, SwfDisplayList dl) {
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineShape4Tag tag, SwfDisplayList dl) {
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineBitsLosslessTag tag, SwfDisplayList dl) {
			AddBitmapToLibrary(
				tag.CharacterId,
				tag.BitmapWidth,
				tag.BitmapHeight,
				tag.ToARGB32());
			return dl;
		}

		public SwfDisplayList Visit(DefineBitsLossless2Tag tag, SwfDisplayList dl) {
			AddBitmapToLibrary(
				tag.CharacterId,
				tag.BitmapWidth,
				tag.BitmapHeight,
				tag.ToARGB32());
			return dl;
		}

		public SwfDisplayList Visit(DefineSpriteTag tag, SwfDisplayList dl) {
			AddSpriteToLibrary(
				tag.SpriteId,
				tag.ControlTags);
			return dl;
		}

		public SwfDisplayList Visit(FileAttributesTag tag, SwfDisplayList dl) {
			return dl;
		}

		public SwfDisplayList Visit(UnknownTag tag, SwfDisplayList dl) {
			return dl;
		}

		public SwfDisplayList Visit(UnsupportedTag tag, SwfDisplayList dl) {
			return dl;
		}

		//
		//
		//

		void AddShapesToLibrary(ushort define_id, SwfShapesWithStyle shapes) {
			var bitmap_styles = shapes.FillStyles.Where(p => p.Type.IsBitmapType);
			var define = new SwfLibraryShapeDefine{
				Bitmaps  = bitmap_styles.Select(p => p.BitmapId    ).ToArray(),
				Matrices = bitmap_styles.Select(p => p.BitmapMatrix).ToArray()
			};
			MainContex.Library.Defines.Add(define_id, define);
		}

		void AddBitmapToLibrary(ushort define_id, int width, int height, byte[] argb32) {
			var define = new SwfLibraryBitmapDefine{
				Width  = width,
				Height = height,
				ARGB32 = argb32
			};
			MainContex.Library.Defines.Add(define_id, define);
		}

		void AddSpriteToLibrary(ushort define_id, SwfControlTags control_tags) {
			var define = new SwfLibrarySpriteDefine{
				ControlTags = control_tags
			};
			MainContex.Library.Defines.Add(define_id, define);
		}

		bool IsSpriteTimelineEnd(SwfDisplaySpriteInstance sprite) {
			var sprite_def = MainContex.Library.FindDefine<SwfLibrarySpriteDefine>(sprite.Id);
			if ( sprite_def != null && sprite.CurrentTag < sprite_def.ControlTags.Tags.Count ) {
				return false;
			}
			var children = sprite.DisplayList.Instances.Values
				.Where (p => p.Type == SwfDisplayInstanceType.Sprite)
				.Select(p => p as SwfDisplaySpriteInstance);
			foreach ( var child in children ) {
				if ( !IsSpriteTimelineEnd(child) ) {
					return false;
				}
			}
			return true;
		}

		void ChildrenNextFrameLooped(SwfDisplayList dl) {
			var sprites = dl.Instances.Values
				.Where (p => p.Type == SwfDisplayInstanceType.Sprite)
				.Select(p => p as SwfDisplaySpriteInstance);
			foreach ( var sprite in sprites ) {
				var sprite_def = MainContex.Library.FindDefine<SwfLibrarySpriteDefine>(sprite.Id);
				if ( sprite_def != null ) {
					if ( IsSpriteTimelineEnd(sprite) ) {
						sprite.Reset();
					}
					var sprite_executer = new SwfContextExecuter(MainContex, sprite.CurrentTag);
					sprite_executer.NextFrame(sprite_def.ControlTags.Tags, sprite.DisplayList);
					sprite.CurrentTag = sprite_executer.CurrentTag;
				}
			}
		}

		void TagToDebugLog(SwfTagBase tag) {
			if ( tag is UnsupportedTag ) {
				Debug.LogError(tag);
			} else if ( tag is UnknownTag ) {
				Debug.LogWarning(tag);
			} else {
				Debug.Log(tag);
			}
		}
	}
}