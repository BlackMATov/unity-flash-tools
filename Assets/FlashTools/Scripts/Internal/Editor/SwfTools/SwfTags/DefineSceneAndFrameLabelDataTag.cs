using System.Collections.Generic;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class DefineSceneAndFrameLabelDataTag : SwfTagBase {
		public struct SceneOffsetData {
			public uint   Offset;
			public string Name;
		}

		public struct FrameLabelData {
			public uint   Number;
			public string Label;
		}

		public List<SceneOffsetData> Scenes;
		public List<FrameLabelData>  Frames;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineSceneAndFrameLabelData; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			return string.Format(
				"DefineSceneAndFrameLabelDataTag. " +
				"Scenes: {0}, Frames: {1}",
				Scenes.Count, Frames.Count);
		}

		public static DefineSceneAndFrameLabelDataTag Create(SwfStreamReader reader) {
			var scenes = new List<SceneOffsetData>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < scenes.Capacity; ++i ) {
				scenes.Add(new SceneOffsetData{
					Offset = reader.ReadEncodedU32(),
					Name   = reader.ReadString()
				});
			}
			var frames = new List<FrameLabelData>((int)reader.ReadEncodedU32());
			for ( var i = 0; i < frames.Capacity; ++i ) {
				frames.Add(new FrameLabelData{
					Number = reader.ReadEncodedU32(),
					Label  = reader.ReadString()
				});
			}
			return new DefineSceneAndFrameLabelDataTag{
				Scenes = scenes,
				Frames = frames};
		}
	}
}