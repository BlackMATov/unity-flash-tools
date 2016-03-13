namespace FlashTools.Internal.SwfTools.SwfTags {
	class SetTabIndexTag : SwfTagBase {
		public ushort Depth;
		public ushort TabIndex;

		public override SwfTagType TagType {
			get { return SwfTagType.SetTabIndex; }
		}

		public override string ToString() {
			return string.Format(
				"SetTabIndexTag. " +
				"Depth: {0}, TabIndex: {1}",
				Depth, TabIndex);
		}

		public static SetTabIndexTag Create(SwfStreamReader reader) {
			var tag      = new SetTabIndexTag();
			tag.Depth    = reader.ReadUInt16();
			tag.TabIndex = reader.ReadUInt16();
			return tag;
		}
	}
}