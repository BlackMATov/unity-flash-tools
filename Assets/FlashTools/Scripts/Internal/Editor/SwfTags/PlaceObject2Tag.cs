using System.Text;

namespace FlashTools.Internal.SwfTags {
	class PlaceObject2Tag : SwfTagBase {
		public bool                  HasClipActions;
		public bool                  HasClipDepth;
		public bool                  HasName;
		public bool                  HasRatio;
		public bool                  HasColorTransform;
		public bool                  HasMatrix;
		public bool                  HasCharacter;
		public bool                  Move;
		public ushort                Depth;
		public ushort                CharacterId;
		public SwfMatrix             Matrix;
		public SwfColorTransformRGBA ColorTransform;
		public ushort                Ratio;
		public string                Name;
		public ushort                ClipDepth;
		public SwfClipActions        ClipActions;

		public override SwfTagType TagType {
			get { return SwfTagType.PlaceObject2; }
		}

		public override string ToString() {
			var sb = new StringBuilder(1024);
			sb.Append("PlaceObject2Tag. ");
			sb.AppendFormat("Move: {0} Depth: {1}", Move, Depth);
			if ( HasCharacter ) {
				sb.AppendFormat(" CharacterId: {0}", CharacterId);
			}
			if ( HasMatrix ) {
				sb.AppendFormat(" Matrix: {0}", Matrix);
			}
			if ( HasColorTransform ) {
				sb.AppendFormat(" ColorTransform: {0}", ColorTransform);
			}
			if ( HasRatio ) {
				sb.AppendFormat(" Ratio: {0}", Ratio);
			}
			if ( HasName ) {
				sb.AppendFormat(" Name: {0}", Name);
			}
			if ( HasClipDepth ) {
				sb.AppendFormat(" ClipDepth: {0}", ClipDepth);
			}
			if ( HasClipActions ) {
				sb.AppendFormat(" ClipActions: {0}", HasClipActions);
			}
			return sb.ToString();
		}

		public static PlaceObject2Tag Create(SwfStreamReader reader) {
			var tag               = new PlaceObject2Tag();
			tag.HasClipActions    = reader.ReadBit();
			tag.HasClipDepth      = reader.ReadBit();
			tag.HasName           = reader.ReadBit();
			tag.HasRatio          = reader.ReadBit();
			tag.HasColorTransform = reader.ReadBit();
			tag.HasMatrix         = reader.ReadBit();
			tag.HasCharacter      = reader.ReadBit();
			tag.Move              = reader.ReadBit();
			tag.Depth             = reader.Reader.ReadUInt16();
			if ( tag.HasCharacter ) {
				tag.CharacterId = reader.Reader.ReadUInt16();
			}
			if ( tag.HasMatrix ) {
				tag.Matrix = SwfMatrix.Read(reader);
			}
			if ( tag.HasColorTransform ) {
				tag.ColorTransform = SwfColorTransformRGBA.Read(reader);
			}
			if ( tag.HasRatio ) {
				tag.Ratio = reader.Reader.ReadUInt16();
			}
			if ( tag.HasName ) {
				tag.Name = reader.ReadString();
			}
			if ( tag.HasClipDepth ) {
				tag.ClipDepth = reader.Reader.ReadUInt16();
			}
			if ( tag.HasClipActions ) {
				tag.ClipActions = SwfClipActions.Read(reader);
			}
			return tag;
		}
	}
}