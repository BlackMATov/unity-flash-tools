namespace FlashTools.Internal.SwfTags {
	class DefineShape4Tag : SwfTagBase {
		public ushort             ShapeId;
		public SwfRect            ShapeBounds;
		public SwfRect            EdgeBounds;
		public SwfShapesWithStyle Shapes;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineShape4; }
		}

		public override string ToString() {
			return string.Format(
				"DefineShape4Tag. " +
				"ShapeId: {0}, ShapeBounds: {1}, EdgeBounds: {2}, Shapes: {3}",
				ShapeId, ShapeBounds, EdgeBounds, Shapes);
		}

		public static DefineShape4Tag Create(SwfStreamReader reader) {
			var tag         = new DefineShape4Tag();
			tag.ShapeId     = reader.Reader.ReadUInt16();
			tag.ShapeBounds = SwfRect.Read(reader);
			tag.EdgeBounds  = SwfRect.Read(reader);
			reader.Reader.ReadByte(); // Skip flags
			tag.Shapes      = SwfShapesWithStyle.Read(reader);
			return tag;
		}
	}
}