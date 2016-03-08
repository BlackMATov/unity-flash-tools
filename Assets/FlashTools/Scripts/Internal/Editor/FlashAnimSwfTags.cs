using System.IO;
using System.Collections.Generic;
using Ionic.Zlib;

namespace FlashTools.Internal {
	enum SwfTagType : ushort {
		//
		// Display list
		//
		PlaceObject = 4,
		PlaceObject2 = 26,
		PlaceObject3 = 70,
		RemoveObject = 5,
		RemoveObject2 = 28,
		ShowFrame = 1,

		//
		// Control
		//
		SetBackgroundColor = 9,
		FrameLabel = 43,
		//Protect = 24,
		End = 0,
		//ExportAssets = 56,
		//ImportAssets = 57,
		//EnableDebugger = 58,
		//EnableDebugger2 = 64,
		//ScriptLimits = 65,
		SetTabIndex = 66,
		//ImportAssets2 = 71,
		//SymbolClass = 76,
		//Metadata = 77,
		DefineScalingGrid = 78,
		DefineSceneAndFrameLabelData = 86,

		//
		// Actions
		//
		//DoAction = 12,
		//DoInitAction = 59,
		//DoABC = 82,

		//
		// Shape
		//
		DefineShape = 2,
		DefineShape2 = 22,
		DefineShape3 = 32,
		DefineShape4 = 83,

		//
		// Bitmaps
		//
		DefineBits = 6,
		JPEGTables = 8,
		DefineBitsJPEG2 = 21,
		DefineBitsJPEG3 = 35,
		DefineBitsLossless = 20,
		DefineBitsLossless2 = 36,
		DefineBitsJPEG4 = 90,

		//
		// Shape Morphing
		//
		//DefineMorphShape = 46,
		//DefineMorphShape2 = 84,

		//
		// Fonts and Text
		//
		//DefineFont = 10,
		//DefineFontInfo = 13,
		//DefineFontInfo2 = 62,
		//DefineFont2 = 48,
		//DefineFont3 = 75,
		//DefineFontAlignZones = 73,
		//DefineFontName = 88,
		//DefineText = 11,
		//DefineText2 = 33,
		//DefineEditText = 37,
		//CSMTextSettings = 74,
		//DefineFont4 = 91,

		//
		// Sounds
		//
		//DefineSound = 14,
		//StartSound = 15,
		//StartSound2 = 89,
		//SoundStreamHead = 18,
		//SoundStreamHead2 = 45,
		//SoundStreamBlock = 19,

		//
		// Buttons
		//
		//DefineButton = 7,
		//DefineButton2 = 34,
		//DefineButtonCxform = 23,
		//DefineButtonSound = 17,

		//
		// Sprites and Movie Clips
		//
		DefineSprite = 39,

		//
		// Video
		//
		//DefineVideoStream = 60,
		//VideoFrame = 61,

		//
		// Metadata
		//
		FileAttributes = 69,
		//EnableTelemetry = 93,
		//DefineBinaryData = 87,
	}

	struct SwfTagData {
		public SwfTagType Type;
		public byte[]     Data;

		public static SwfTagData Read(SwfStreamReader reader) {
			var type_and_size = reader.Reader.ReadUInt16();
			var type          = (SwfTagType)(type_and_size >> 6);
			var short_size    = type_and_size & 0x3f;
			var size          = short_size < 0x3f ? short_size : reader.Reader.ReadInt32();
			var tag_data      = reader.Reader.ReadBytes(size);
			return new SwfTagData{Type = type, Data = tag_data};
		}
	}

	abstract class SwfTagBase {
		public abstract SwfTagType TagType { get; }
		public static SwfTagBase Create(SwfTagData tag_data) {
			var stream = new MemoryStream(tag_data.Data);
			var reader = new SwfStreamReader(stream);
			switch ( tag_data.Type ) {
			case SwfTagType.PlaceObject:                  return PlaceObjectTag.Create(reader);
			case SwfTagType.PlaceObject2:                 return PlaceObject2Tag.Create(reader);
			case SwfTagType.PlaceObject3:                 return PlaceObject3Tag.Create(reader);
			case SwfTagType.RemoveObject:                 return RemoveObjectTag.Create(reader);
			case SwfTagType.RemoveObject2:                return RemoveObject2Tag.Create(reader);
			case SwfTagType.ShowFrame:                    return ShowFrameTag.Create(reader);
			case SwfTagType.SetBackgroundColor:           return SetBackgroundColorTag.Create(reader);
			case SwfTagType.FrameLabel:                   return FrameLabelTag.Create(reader);
			case SwfTagType.End:                          return EndTag.Create(reader);
			case SwfTagType.SetTabIndex:                  return SetTabIndexTag.Create(reader);
			case SwfTagType.DefineScalingGrid:            return DefineScalingGridTag.Create(reader);
			case SwfTagType.DefineSceneAndFrameLabelData: return DefineSceneAndFrameLabelDataTag.Create(reader);
			case SwfTagType.DefineShape:                  return DefineShapeTag.Create(reader);
			case SwfTagType.DefineShape2:                 return DefineShape2Tag.Create(reader);
			case SwfTagType.DefineShape3:                 return DefineShape3Tag.Create(reader);
			case SwfTagType.DefineShape4:                 return DefineShape4Tag.Create(reader);
			//case DefineBits:
			//case JPEGTables:
			//case DefineBitsJPEG2:
			//case DefineBitsJPEG3:
			case SwfTagType.DefineBitsLossless:           return DefineBitsLosslessTag.Create(reader);
			case SwfTagType.DefineBitsLossless2:          return DefineBitsLossless2Tag.Create(reader);
			//case DefineBitsJPEG4:
			case SwfTagType.DefineSprite:                 return DefineSpriteTag.Create(reader);
			case SwfTagType.FileAttributes:               return FileAttributesTag.Create(reader);
			default:                                      return UnknownTag.Create(tag_data.Type);
			}
		}
	}

	class PlaceObjectTag : SwfTagBase {
		public ushort               CharacterId;
		public ushort               Depth;
		public SwfMatrix            Matrix;
		public SwfColorTransformRGB ColorTransform;

		public override string ToString() {
			return string.Format(
				"CharacterId: {0}, Depth: {1}, Matrix: {2}, ColorTransform: {3}",
				CharacterId, Depth, Matrix, ColorTransform);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.PlaceObject; }
		}

		public static PlaceObjectTag Create(SwfStreamReader reader) {
			var tag         = new PlaceObjectTag();
			tag.CharacterId = reader.Reader.ReadUInt16();
			tag.Depth       = reader.Reader.ReadUInt16();
			tag.Matrix      = SwfMatrix.Read(reader);
			if ( !reader.IsEOF ) {
				tag.ColorTransform = SwfColorTransformRGB.Read(reader);
			}
			return tag;
		}
	}

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

		public override string ToString() {
			return string.Format(
				"CharacterId: {0}, Depth: {1}, Matrix: {2}, ColorTransform: {3}",
				CharacterId, Depth, Matrix, ColorTransform);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.PlaceObject2; }
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

	class PlaceObject3Tag : SwfTagBase {
		public bool                  HasClipActions;
		public bool                  HasClipDepth;
		public bool                  HasName;
		public bool                  HasRatio;
		public bool                  HasColorTransform;
		public bool                  HasMatrix;
		public bool                  HasCharacter;
		public bool                  Move;
		public bool                  OpaqueBackground;
		public bool                  HasVisible;
		public bool                  HasImage;
		public bool                  HasClassName;
		public bool                  HasCacheAsBitmap;
		public bool                  HasBlendMode;
		public bool                  HasFilterList;
		public ushort                Depth;
		public string                ClassName;
		public ushort                CharacterId;
		public SwfMatrix             Matrix;
		public SwfColorTransformRGBA ColorTransform;
		public ushort                Ratio;
		public string                Name;
		public ushort                ClipDepth;
		public SwfSurfaceFilters     SurfaceFilters;
		public SwfBlendMode          BlendMode;
		public byte                  BitmapCache;
		public byte                  Visible;
		public SwfRGBA               BackgroundColor;
		public SwfClipActions        ClipActions;

		public override string ToString() {
			return string.Format(
				"CharacterId: {0}, Depth: {1}, Matrix: {2}, ColorTransform: {3}",
				CharacterId, Depth, Matrix, ColorTransform);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.PlaceObject3; }
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
			tag.Depth             = reader.Reader.ReadUInt16();
			if ( tag.HasCharacter || (tag.HasImage && tag.HasCharacter) ) {
				tag.ClassName = reader.ReadString();
			}
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
			if ( tag.HasFilterList ) {
				tag.SurfaceFilters = SwfSurfaceFilters.Read(reader);
			}
			if ( tag.HasBlendMode ) {
				tag.BlendMode = (SwfBlendMode)reader.Reader.ReadByte();
			}
			if ( tag.HasCacheAsBitmap ) {
				tag.BitmapCache = reader.Reader.ReadByte();
			}
			if ( tag.HasVisible ) {
				tag.Visible         = reader.Reader.ReadByte();
				tag.BackgroundColor = SwfRGBA.Read(reader);
			}
			if ( tag.HasClipActions ) {
				tag.ClipActions = SwfClipActions.Read(reader);
			}
			return tag;
		}
	}

	class RemoveObjectTag : SwfTagBase {
		public ushort CharacterId;
		public ushort Depth;

		public override string ToString() {
			return string.Format("CharacterId: {0}, Depth: {1}", CharacterId, Depth);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.RemoveObject; }
		}

		public static RemoveObjectTag Create(SwfStreamReader reader) {
			var tag = new RemoveObjectTag();
			tag.CharacterId = reader.Reader.ReadUInt16();
			tag.Depth       = reader.Reader.ReadUInt16();
			return tag;
		}
	}

	class RemoveObject2Tag : SwfTagBase {
		public ushort Depth;

		public override string ToString() {
			return string.Format("Depth: {0}", Depth);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.RemoveObject2; }
		}

		public static RemoveObject2Tag Create(SwfStreamReader reader) {
			var tag = new RemoveObject2Tag();
			tag.Depth = reader.Reader.ReadUInt16();
			return tag;
		}
	}

	class ShowFrameTag : SwfTagBase {
		public override string ToString() {
			return "ShowFrameTag";
		}

		public override SwfTagType TagType {
			get { return SwfTagType.ShowFrame; }
		}

		public static ShowFrameTag Create(SwfStreamReader reader) {
			return new ShowFrameTag();
		}
	}

	class SetBackgroundColorTag : SwfTagBase {
		public SwfRGB BackgroundColor;

		public override string ToString() {
			return string.Format("BackgroundColor: {0}", BackgroundColor);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.SetBackgroundColor; }
		}

		public static SetBackgroundColorTag Create(SwfStreamReader reader) {
			var tag = new SetBackgroundColorTag();
			tag.BackgroundColor = SwfRGB.Read(reader);
			return tag;
		}
	}

	class FrameLabelTag : SwfTagBase {
		public string Name;
		public byte   AnchorFlag;

		public override string ToString() {
			return string.Format("Name: {0}, AnchorFlag: {1}", Name, AnchorFlag);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.FrameLabel; }
		}

		public static FrameLabelTag Create(SwfStreamReader reader) {
			var tag = new FrameLabelTag();
			tag.Name = reader.ReadString();
			if ( !reader.IsEOF ) {
				tag.AnchorFlag = reader.Reader.ReadByte();
			}
			return tag;
		}
	}

	class EndTag : SwfTagBase {
		public override string ToString() {
			return "EndTag";
		}

		public override SwfTagType TagType {
			get { return SwfTagType.End; }
		}

		public static EndTag Create(SwfStreamReader reader) {
			return new EndTag();
		}
	}

	class SetTabIndexTag : SwfTagBase {
		public ushort Depth;
		public ushort TabIndex;

		public override string ToString() {
			return string.Format("Depth: {0}, TabIndex: {1}", Depth, TabIndex);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.SetTabIndex; }
		}

		public static SetTabIndexTag Create(SwfStreamReader reader) {
			var tag = new SetTabIndexTag();
			tag.Depth    = reader.Reader.ReadUInt16();
			tag.TabIndex = reader.Reader.ReadUInt16();
			return tag;
		}
	}

	class DefineScalingGridTag : SwfTagBase {
		public ushort  CharacterId;
		public SwfRect Splitter;

		public override string ToString() {
			return string.Format("CharacterId: {0}, Splitter: {1}", CharacterId, Splitter);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineScalingGrid; }
		}

		public static DefineScalingGridTag Create(SwfStreamReader reader) {
			var tag = new DefineScalingGridTag();
			tag.CharacterId = reader.Reader.ReadUInt16();
			tag.Splitter    = SwfRect.Read(reader);
			return tag;
		}
	}

	class DefineSceneAndFrameLabelDataTag : SwfTagBase {
		public List<SceneOffsetData> Scenes = new List<SceneOffsetData>();
		public List<FrameLabelData>  Frames = new List<FrameLabelData>();

		public override string ToString() {
			return string.Format("Scenes: {0}, Frames: {1}", Scenes.Count, Frames.Count);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineSceneAndFrameLabelData; }
		}

		public static DefineSceneAndFrameLabelDataTag Create(SwfStreamReader reader) {
			var tag = new DefineSceneAndFrameLabelDataTag();
			var scenes = reader.ReadEncodedU32();
			for ( var i = 0; i < scenes; ++i ) {
				tag.Scenes.Add(new SceneOffsetData {
					Offset = reader.ReadEncodedU32(),
					Name   = reader.ReadString()
				});
			}
			var frames = reader.ReadEncodedU32();
			for ( var i = 0; i < frames; ++i ) {
				tag.Frames.Add(new FrameLabelData {
					Number = reader.ReadEncodedU32(),
					Label  = reader.ReadString()
				});
			}
			return tag;
		}
	}

	class DefineShapeTag : SwfTagBase {
		public ushort  ShapeID;
		public SwfRect ShapeBounds;

		public override string ToString() {
			return string.Format("ShapeID: {0}, ShapeBounds: {1}", ShapeID, ShapeBounds);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineShape; }
		}

		public static DefineShapeTag Create(SwfStreamReader reader) {
			var tag         = new DefineShapeTag();
			tag.ShapeID     = reader.Reader.ReadUInt16();
			tag.ShapeBounds = SwfRect.Read(reader);
			//tag.Shapes    = SwfShapes.Read(reader);
			return tag;
		}
	}

	class DefineShape2Tag : SwfTagBase {
		public ushort  ShapeID;
		public SwfRect ShapeBounds;

		public override string ToString() {
			return string.Format("ShapeID: {0}, ShapeBounds: {1}", ShapeID, ShapeBounds);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineShape2; }
		}

		public static DefineShape2Tag Create(SwfStreamReader reader) {
			var tag         = new DefineShape2Tag();
			tag.ShapeID     = reader.Reader.ReadUInt16();
			tag.ShapeBounds = SwfRect.Read(reader);
			//tag.Shapes    = SwfShapes.Read(reader);
			return tag;
		}
	}

	class DefineShape3Tag : SwfTagBase {
		public ushort  ShapeID;
		public SwfRect ShapeBounds;

		public override string ToString() {
			return string.Format("ShapeID: {0}, ShapeBounds: {1}", ShapeID, ShapeBounds);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineShape3; }
		}

		public static DefineShape3Tag Create(SwfStreamReader reader) {
			var tag         = new DefineShape3Tag();
			tag.ShapeID     = reader.Reader.ReadUInt16();
			tag.ShapeBounds = SwfRect.Read(reader);
			//tag.Shapes    = SwfShapes.Read(reader);
			return tag;
		}
	}

	class DefineShape4Tag : SwfTagBase {
		public ushort  ShapeID;
		public SwfRect ShapeBounds;
		public SwfRect EdgeBounds;

		public override string ToString() {
			return string.Format(
				"ShapeID: {0}, ShapeBounds: {1}, EdgeBounds: {2}",
				ShapeID, ShapeBounds, EdgeBounds);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineShape4; }
		}

		public static DefineShape4Tag Create(SwfStreamReader reader) {
			var tag                   = new DefineShape4Tag();
			tag.ShapeID               = reader.Reader.ReadUInt16();
			tag.ShapeBounds           = SwfRect.Read(reader);
			tag.EdgeBounds            = SwfRect.Read(reader);
			//tag.Flags               = reader.Reader.ReadByte();
			//tag.Shapes              = SwfShapes.Read(reader);
			return tag;
		}
	}

	class DefineBitsLosslessTag : SwfTagBase {
		public ushort CharacterId;
		public byte   BitmapFormat;
		public ushort BitmapWidth;
		public ushort BitmapHeight;
		public byte   BitmapColorTableSize;
		public byte[] ZlibBitmapData;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineBitsLossless; }
		}

		public override string ToString() {
			return string.Format(
				"CharacterId: {0}, BitmapFormat: {1}, Width: {2}, Height: {3}",
				CharacterId, BitmapFormat, BitmapWidth, BitmapHeight);
		}

		public static DefineBitsLosslessTag Create(SwfStreamReader reader) {
			var tag          = new DefineBitsLosslessTag();
			tag.CharacterId  = reader.Reader.ReadUInt16();
			tag.BitmapFormat = reader.Reader.ReadByte();
			tag.BitmapWidth  = reader.Reader.ReadUInt16();
			tag.BitmapHeight = reader.Reader.ReadUInt16();
			if ( tag.BitmapFormat == 3 ) {
				tag.BitmapColorTableSize = reader.Reader.ReadByte();
			}
			tag.ZlibBitmapData = reader.ReadRest();
			return tag;
		}
	}

	class DefineBitsLossless2Tag : SwfTagBase {
		public ushort CharacterId;
		public byte   BitmapFormat;
		public ushort BitmapWidth;
		public ushort BitmapHeight;
		public byte   BitmapColorTableSize;
		public byte[] ZlibBitmapData;

		public override SwfTagType TagType {
			get { return SwfTagType.DefineBitsLossless2; }
		}

		public override string ToString() {
			return string.Format(
				"CharacterId: {0}, BitmapFormat: {1}, Width: {2}, Height: {3}",
				CharacterId, BitmapFormat, BitmapWidth, BitmapHeight);
		}

		public static DefineBitsLossless2Tag Create(SwfStreamReader reader) {
			var tag          = new DefineBitsLossless2Tag();
			tag.CharacterId  = reader.Reader.ReadUInt16();
			tag.BitmapFormat = reader.Reader.ReadByte();
			tag.BitmapWidth  = reader.Reader.ReadUInt16();
			tag.BitmapHeight = reader.Reader.ReadUInt16();
			if ( tag.BitmapFormat == 3 ) {
				tag.BitmapColorTableSize = reader.Reader.ReadByte();
			}
			tag.ZlibBitmapData = reader.ReadRest();
			return tag;
		}
	}

	class DefineSpriteTag : SwfTagBase {
		public ushort           SpriteId;
		public ushort           FrameCount;
		public List<SwfTagBase> ControlTags;

		public override string ToString() {
			return string.Format(
				"SpriteId: {0}, FrameCount: {1}, ControlTags: {2}",
				SpriteId, FrameCount, ControlTags.Count);
		}

		public override SwfTagType TagType {
			get { return SwfTagType.DefineSprite; }
		}

		public static DefineSpriteTag Create(SwfStreamReader reader) {
			var tag         = new DefineSpriteTag();
			tag.SpriteId    = reader.Reader.ReadUInt16();
			tag.FrameCount  = reader.Reader.ReadUInt16();
			tag.ControlTags = new List<SwfTagBase>();
			SwfTagBase sub_tag;
			do {
				var sub_tag_data = SwfTagData.Read(reader);
				sub_tag = SwfTagBase.Create(sub_tag_data);
				if ( sub_tag != null ) {
					tag.ControlTags.Add(sub_tag);
				}
			} while ( sub_tag != null && sub_tag.TagType != SwfTagType.End );
			return tag;
		}
	}

	class FileAttributesTag : SwfTagBase {
		public override string ToString() {
			return "FileAttributesTag";
		}

		public override SwfTagType TagType {
			get { return SwfTagType.FileAttributes; }
		}

		public static FileAttributesTag Create(SwfStreamReader reader) {
			return new FileAttributesTag();
		}
	}

	class UnknownTag : SwfTagBase {
		SwfTagType Type;

		public override string ToString() {
			return "UnknownTag";
		}

		public override SwfTagType TagType {
			get { return Type; }
		}

		public static UnknownTag Create(SwfTagType type) {
			var tag = new UnknownTag();
			tag.Type = type;
			return tag;
		}
	}
}