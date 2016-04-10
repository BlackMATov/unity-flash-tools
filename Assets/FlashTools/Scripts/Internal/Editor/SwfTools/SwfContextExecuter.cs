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
				tag.AcceptVistor(this, dl);
				if ( tag.TagType == SwfTagType.ShowFrame ) {
					return true;
				}
			}
			return false;
		}

		public SwfDisplayList Visit(PlaceObjectTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			return dl;
		}

		public SwfDisplayList Visit(PlaceObject2Tag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			var is_shape  = tag.HasCharacter
				? MainContex.Library.HasDefine<SwfLibraryShapeDefine >(tag.CharacterId)
				: false;
			var is_sprite = tag.HasCharacter
				? MainContex.Library.HasDefine<SwfLibrarySpriteDefine>(tag.CharacterId)
				: false;
			if ( !tag.Move && tag.HasCharacter ) { // new character
				SwfDisplayInst new_inst = null;
				if ( is_shape ) {
					new_inst = new SwfDisplayShapeInst();
				} else if ( is_sprite ) {
					new_inst = new SwfDisplaySpriteInst();
				}
				if ( new_inst != null ) {
					new_inst.Id             = tag.CharacterId;
					new_inst.Depth          = tag.Depth;
					new_inst.Matrix         = tag.HasMatrix         ? tag.Matrix         : SwfMatrix.identity;
					new_inst.ColorTransform = tag.HasColorTransform ? tag.ColorTransform : SwfColorTransform.identity;
					dl.Insts.Add(new_inst.Depth, new_inst);
				}
			} else if ( tag.Move && !tag.HasCharacter ) { // move character
			} else if ( tag.Move &&  tag.HasCharacter ) { // replace character
			}
			return dl;
		}

		public SwfDisplayList Visit(PlaceObject3Tag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			return dl;
		}

		public SwfDisplayList Visit(RemoveObjectTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			dl.Insts.Remove(tag.Depth);
			return dl;
		}

		public SwfDisplayList Visit(RemoveObject2Tag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			dl.Insts.Remove(tag.Depth);
			return dl;
		}

		public SwfDisplayList Visit(ShowFrameTag tag, SwfDisplayList dl) {
			Debug.LogError(tag);
			var sprites = dl.Insts.Values
				.Where (p => p.Type == SwfDisplayInstType.Sprite)
				.Select(p => p as SwfDisplaySpriteInst);
			foreach ( var sprite in sprites ) {
				var sprite_def = MainContex.Library.FindDefine<SwfLibrarySpriteDefine>(sprite.Id);
				if ( sprite_def != null ) {
					var sprite_executer = new SwfContextExecuter(MainContex, sprite.CurrentTag);
					sprite_executer.NextFrame(sprite_def.ControlTags.Tags, sprite.DisplayList);
					sprite.CurrentTag = sprite_executer.CurrentTag;
				}
			}
			return dl;
		}

		public SwfDisplayList Visit(SetBackgroundColorTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			return dl;
		}

		public SwfDisplayList Visit(FrameLabelTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			dl.FrameName = tag.Name;
			return dl;
		}

		public SwfDisplayList Visit(EndTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			return dl;
		}

		public SwfDisplayList Visit(DefineSceneAndFrameLabelDataTag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			return dl;
		}

		public SwfDisplayList Visit(DefineShapeTag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineShape2Tag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineShape3Tag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineShape4Tag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			AddShapesToLibrary(tag.ShapeId, tag.Shapes);
			return dl;
		}

		public SwfDisplayList Visit(DefineBitsLosslessTag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			AddZlibBitmapToLibrary(tag.CharacterId);
			return dl;
		}

		public SwfDisplayList Visit(DefineBitsLossless2Tag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			AddZlibBitmapToLibrary(tag.CharacterId);
			return dl;
		}

		public SwfDisplayList Visit(DefineSpriteTag tag, SwfDisplayList dl) {
			Debug.LogWarning(tag);
			var define = new SwfLibrarySpriteDefine{
				ControlTags = tag.ControlTags
			};
			MainContex.Library.Defines.Add(tag.SpriteId, define);
			return dl;
		}

		public SwfDisplayList Visit(FileAttributesTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			return dl;
		}

		public SwfDisplayList Visit(UnknownTag tag, SwfDisplayList dl) {
			Debug.Log(tag);
			return dl;
		}

		//
		//
		//

		void AddShapesToLibrary(ushort define_id, SwfShapesWithStyle shapes) {
			var bitmap_styles = shapes.FillStyles.Where(p => p.Type.IsBitmapType);
			var define = new SwfLibraryShapeDefine{
				Bitmaps  = bitmap_styles.Select(p => p.BitmapId).ToArray(),
				Matrices = bitmap_styles.Select(p => p.BitmapMatrix).ToArray()
			};
			MainContex.Library.Defines.Add(define_id, define);
		}

		void AddZlibBitmapToLibrary(ushort define_id) {
			//TODO: IMPLME
			var define = new SwfLibraryBitmapDefine{};
			MainContex.Library.Defines.Add(define_id, define);
		}
	}
}