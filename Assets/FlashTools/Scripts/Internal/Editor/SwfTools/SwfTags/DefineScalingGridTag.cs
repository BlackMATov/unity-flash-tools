namespace FlashTools.Internal.SwfTools.SwfTags {
	class DefineScalingGridTag : SwfTagBase {
		public ushort  CharacterId;
		public SwfRect Splitter;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineScalingGrid; }
		}

		public override string ToString() {
			return string.Format(
				"DefineScalingGridTag. " +
				"CharacterId: {0}, Splitter: {1}",
				CharacterId, Splitter);
		}

		public static DefineScalingGridTag Create(SwfStreamReader reader) {
			var tag         = new DefineScalingGridTag();
			tag.CharacterId = reader.ReadUInt16();
			tag.Splitter    = SwfRect.Read(reader);
			return tag;
		}
	}
}