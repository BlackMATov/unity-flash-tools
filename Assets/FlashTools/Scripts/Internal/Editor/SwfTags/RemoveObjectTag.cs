﻿namespace FlashTools.Internal.SwfTags {
	class RemoveObjectTag : SwfTagBase {
		public ushort CharacterId;
		public ushort Depth;

		public override SwfTagType TagType {
			get { return SwfTagType.RemoveObject; }
		}

		public override string ToString() {
			return string.Format(
				"RemoveObjectTag. " +
				"CharacterId: {0}, Depth: {1}",
				CharacterId, Depth);
		}

		public static RemoveObjectTag Create(SwfStreamReader reader) {
			var tag         = new RemoveObjectTag();
			tag.CharacterId = reader.Reader.ReadUInt16();
			tag.Depth       = reader.Reader.ReadUInt16();
			return tag;
		}
	}
}
