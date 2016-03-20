using System.Text;
using FlashTools.Internal.SwfTools.SwfTypes;

namespace FlashTools.Internal.SwfTools.SwfTags {
	public class PlaceObject3Tag : SwfTagBase {
		public bool              HasClipActions;
		public bool              HasClipDepth;
		public bool              HasName;
		public bool              HasRatio;
		public bool              HasColorTransform;
		public bool              HasMatrix;
		public bool              HasCharacter;
		public bool              Move;
		public bool              OpaqueBackground;
		public bool              HasVisible;
		public bool              HasImage;
		public bool              HasClassName;
		public bool              HasCacheAsBitmap;
		public bool              HasBlendMode;
		public bool              HasFilterList;
		public ushort            Depth;
		public string            ClassName;
		public ushort            CharacterId;
		public SwfMatrix         Matrix;
		public SwfColorTransform ColorTransform;
		public ushort            Ratio;
		public string            Name;
		public ushort            ClipDepth;
		public SwfSurfaceFilters SurfaceFilters;
		public SwfBlendMode      BlendMode;
		public byte              BitmapCache;
		public byte              Visible;
		public SwfColor          BackgroundColor;
		public SwfClipActions    ClipActions;

		public override SwfTagType TagType {
			get { return SwfTagType.PlaceObject3; }
		}

		public override TResult AcceptVistor<TArg, TResult>(SwfTagVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public override string ToString() {
			var sb = new StringBuilder(1024);
			sb.Append("PlaceObject3Tag. ");
			sb.AppendFormat("Move: {0} Depth: {1}", Move, Depth);
			if ( HasCharacter ) {
				sb.AppendFormat(", CharacterId: {0}", CharacterId);
			}
			if ( HasMatrix ) {
				sb.AppendFormat(", Matrix: {0}", Matrix);
			}
			if ( HasColorTransform ) {
				sb.AppendFormat(", ColorTransform: {0}", ColorTransform);
			}
			if ( HasRatio ) {
				sb.AppendFormat(", Ratio: {0}", Ratio);
			}
			if ( HasName ) {
				sb.AppendFormat(", Name: {0}", Name);
			}
			if ( HasClipDepth ) {
				sb.AppendFormat(", ClipDepth: {0}", ClipDepth);
			}
			if ( HasClipActions ) {
				sb.AppendFormat(", ClipActions: {0}", HasClipActions);
			}
			return sb.ToString();
		}

		public static PlaceObject3Tag Create(SwfStreamReader reader) {
			var tag               = new PlaceObject3Tag();
			tag.HasClipActions    = reader.ReadBit();
			tag.HasClipDepth      = reader.ReadBit();
			tag.HasName           = reader.ReadBit();
			tag.HasRatio          = reader.ReadBit();
			tag.HasColorTransform = reader.ReadBit();
			tag.HasMatrix         = reader.ReadBit();
			tag.HasCharacter      = reader.ReadBit();
			tag.Move              = reader.ReadBit();
			reader.ReadBit(); // reserved
			tag.OpaqueBackground  = reader.ReadBit();
			tag.HasVisible        = reader.ReadBit();
			tag.HasImage          = reader.ReadBit();
			tag.HasClassName      = reader.ReadBit();
			tag.HasCacheAsBitmap  = reader.ReadBit();
			tag.HasBlendMode      = reader.ReadBit();
			tag.HasFilterList     = reader.ReadBit();
			tag.Depth             = reader.ReadUInt16();
			if ( tag.HasCharacter || (tag.HasImage && tag.HasCharacter) ) {
				tag.ClassName = reader.ReadString();
			}
			if ( tag.HasCharacter ) {
				tag.CharacterId = reader.ReadUInt16();
			}
			if ( tag.HasMatrix ) {
				tag.Matrix = SwfMatrix.Read(reader);
			}
			if ( tag.HasColorTransform ) {
				tag.ColorTransform = SwfColorTransform.Read(reader, true);
			}
			if ( tag.HasRatio ) {
				tag.Ratio = reader.ReadUInt16();
			}
			if ( tag.HasName ) {
				tag.Name = reader.ReadString();
			}
			if ( tag.HasClipDepth ) {
				tag.ClipDepth = reader.ReadUInt16();
			}
			if ( tag.HasFilterList ) {
				tag.SurfaceFilters = SwfSurfaceFilters.Read(reader);
			}
			if ( tag.HasBlendMode ) {
				tag.BlendMode = SwfBlendMode.Read(reader);
			}
			if ( tag.HasCacheAsBitmap ) {
				tag.BitmapCache = reader.ReadByte();
			}
			if ( tag.HasVisible ) {
				tag.Visible         = reader.ReadByte();
				tag.BackgroundColor = SwfColor.Read(reader, true);
			}
			if ( tag.HasClipActions ) {
				tag.ClipActions = SwfClipActions.Read(reader);
			}
			return tag;
		}
	}
}