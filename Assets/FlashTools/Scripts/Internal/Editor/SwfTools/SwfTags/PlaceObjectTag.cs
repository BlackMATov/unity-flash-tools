using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools.SwfTags {
	class PlaceObjectTag : SwfTagBase {
		public ushort               CharacterId;
		public ushort               Depth;
		public SwfMatrix            Matrix;
		public SwfColorTransformRGB ColorTransform;

		public override SwfTagType TagType {
			get { return SwfTagType.PlaceObject; }
		}

		public override string ToString() {
			return string.Format(
				"PlaceObjectTag. " +
				"CharacterId: {0}, Depth: {1}, Matrix: {2}, ColorTransform: {3}",
				CharacterId, Depth, Matrix, ColorTransform);
		}

		public static PlaceObjectTag Create(SwfStreamReader reader) {
			var tag         = new PlaceObjectTag();
			tag.CharacterId = reader.ReadUInt16();
			tag.Depth       = reader.ReadUInt16();
			tag.Matrix      = SwfMatrix.Read(reader);
			if ( !reader.IsEOF ) {
				tag.ColorTransform = SwfColorTransformRGB.Read(reader);
			}
			return tag;
		}
	}
}