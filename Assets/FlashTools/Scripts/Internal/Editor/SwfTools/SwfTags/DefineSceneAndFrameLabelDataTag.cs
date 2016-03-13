using System.Collections.Generic;

namespace FlashTools.Internal.SwfTools.SwfTags {
	class DefineSceneAndFrameLabelDataTag : SwfTagBase {
		public struct SceneOffsetData {
			public uint   Offset;
			public string Name;
		}

		public struct FrameLabelData {
			public uint   Number;
			public string Label;
		}

		public List<SceneOffsetData> Scenes = new List<SceneOffsetData>();
		public List<FrameLabelData>  Frames = new List<FrameLabelData>();

		public override SwfTagType TagType {
			get { return SwfTagType.DefineSceneAndFrameLabelData; }
		}

		public override string ToString() {
			return string.Format(
				"DefineSceneAndFrameLabelDataTag. " +
				"Scenes: {0}, Frames: {1}",
				Scenes.Count, Frames.Count);
		}

		public static DefineSceneAndFrameLabelDataTag Create(SwfStreamReader reader) {
			var tag = new DefineSceneAndFrameLabelDataTag();
			var scenes = reader.ReadEncodedU32();
			for ( var i = 0; i < scenes; ++i ) {
				tag.Scenes.Add(new SceneOffsetData{
					Offset = reader.ReadEncodedU32(),
					Name   = reader.ReadString()
				});
			}
			var frames = reader.ReadEncodedU32();
			for ( var i = 0; i < frames; ++i ) {
				tag.Frames.Add(new FrameLabelData{
					Number = reader.ReadEncodedU32(),
					Label  = reader.ReadString()
				});
			}
			return tag;
		}
	}
}