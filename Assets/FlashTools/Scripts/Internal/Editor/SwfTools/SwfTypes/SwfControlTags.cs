using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTags;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfControlTags {
		public List<SwfTagBase> Tags;

		public static SwfControlTags Read(SwfStreamReader reader) {
			var control_tags = new SwfControlTags();
			control_tags.Tags = new List<SwfTagBase>();
			while ( true ) {
				var tag = SwfTagBase.Read(reader);
				if ( tag.TagType == SwfTagType.End ) {
					break;
				}
				control_tags.Tags.Add(tag);
			}
			return control_tags;
		}

		public override string ToString() {
			return string.Format(
				"SwfControlTags. Tags: {0}",
				Tags.Count);
		}
	}
}