namespace FlashTools.Internal.SwfTags {
	class DefineShapeTag : SwfTagBase {
		public ushort             ShapeId;
		public SwfRect            ShapeBounds;
		public SwfShapesWithStyle Shapes;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineShape; }
		}

		public override string ToString() {
			return string.Format(
				"DefineShapeTag. " +
				"ShapeId: {0}, ShapeBounds: {1}, Shapes: {2}",
				ShapeId, ShapeBounds, Shapes);
		}

		public static DefineShapeTag Create(SwfStreamReader reader) {
			var tag         = new DefineShapeTag();
			tag.ShapeId     = reader.Reader.ReadUInt16();
			tag.ShapeBounds = SwfRect.Read(reader);
			tag.Shapes      = SwfShapesWithStyle.Read(reader);
			return tag;
		}
	}
}