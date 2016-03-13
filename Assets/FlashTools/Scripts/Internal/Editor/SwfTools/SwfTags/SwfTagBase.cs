namespace FlashTools.Internal.SwfTools.SwfTags {
	enum SwfTagType {
		// -----------------------------
		// Display list
		// -----------------------------

		PlaceObject = 4,
		PlaceObject2 = 26,
		PlaceObject3 = 70,
		RemoveObject = 5,
		RemoveObject2 = 28,
		ShowFrame = 1,

		// -----------------------------
		// Control
		// -----------------------------

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

		// -----------------------------
		// Actions
		// -----------------------------

		//DoAction = 12,
		//DoInitAction = 59,
		//DoABC = 82,

		// -----------------------------
		// Shape
		// -----------------------------

		DefineShape = 2,
		DefineShape2 = 22,
		DefineShape3 = 32,
		DefineShape4 = 83,

		// -----------------------------
		// Bitmaps
		// -----------------------------

		//DefineBits = 6,
		//JPEGTables = 8,
		//DefineBitsJPEG2 = 21,
		//DefineBitsJPEG3 = 35,
		DefineBitsLossless = 20,
		DefineBitsLossless2 = 36,
		//DefineBitsJPEG4 = 90,

		// -----------------------------
		// Shape Morphing
		// -----------------------------

		//DefineMorphShape = 46,
		//DefineMorphShape2 = 84,

		// -----------------------------
		// Fonts and Text
		// -----------------------------

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

		// -----------------------------
		// Sounds
		// -----------------------------

		//DefineSound = 14,
		//StartSound = 15,
		//StartSound2 = 89,
		//SoundStreamHead = 18,
		//SoundStreamHead2 = 45,
		//SoundStreamBlock = 19,

		// -----------------------------
		// Buttons
		// -----------------------------

		//DefineButton = 7,
		//DefineButton2 = 34,
		//DefineButtonCxform = 23,
		//DefineButtonSound = 17,

		// -----------------------------
		// Sprites and Movie Clips
		// -----------------------------

		DefineSprite = 39,

		// -----------------------------
		// Video
		// -----------------------------

		//DefineVideoStream = 60,
		//VideoFrame = 61,

		// -----------------------------
		// Metadata
		// -----------------------------

		FileAttributes = 69,
		//EnableTelemetry = 93,
		//DefineBinaryData = 87,

		// -----------------------------
		// Unknown
		// -----------------------------

		Unknown
	}

	abstract class SwfTagBase {
		struct SwfTagData {
			public int    TagId;
			public byte[] TagData;
		}

		public abstract SwfTagType TagType { get; }

		public static SwfTagBase Read(SwfStreamReader reader) {
			var type_and_size = reader.ReadUInt16();
			var tag_id        = type_and_size >> 6;
			var short_size    = type_and_size & 0x3f;
			var size          = short_size < 0x3f ? short_size : reader.ReadInt32();
			var tag_data      = reader.ReadBytes(size);
			return Create(new SwfTagData{
				TagId   = tag_id,
				TagData = tag_data});
		}

		static SwfTagBase Create(SwfTagData tag_data) {
			var reader = new SwfStreamReader(tag_data.TagData);
			switch ( tag_data.TagId ) {
			case (int)SwfTagType.PlaceObject:                  return PlaceObjectTag.Create(reader);
			case (int)SwfTagType.PlaceObject2:                 return PlaceObject2Tag.Create(reader);
			case (int)SwfTagType.PlaceObject3:                 return PlaceObject3Tag.Create(reader);
			case (int)SwfTagType.RemoveObject:                 return RemoveObjectTag.Create(reader);
			case (int)SwfTagType.RemoveObject2:                return RemoveObject2Tag.Create(reader);
			case (int)SwfTagType.ShowFrame:                    return ShowFrameTag.Create(reader);
			case (int)SwfTagType.SetBackgroundColor:           return SetBackgroundColorTag.Create(reader);
			case (int)SwfTagType.FrameLabel:                   return FrameLabelTag.Create(reader);
			case (int)SwfTagType.End:                          return EndTag.Create(reader);
			case (int)SwfTagType.SetTabIndex:                  return SetTabIndexTag.Create(reader);
			case (int)SwfTagType.DefineScalingGrid:            return DefineScalingGridTag.Create(reader);
			case (int)SwfTagType.DefineSceneAndFrameLabelData: return DefineSceneAndFrameLabelDataTag.Create(reader);
			case (int)SwfTagType.DefineShape:                  return DefineShapeTag.Create(reader);
			case (int)SwfTagType.DefineShape2:                 return DefineShape2Tag.Create(reader);
			case (int)SwfTagType.DefineShape3:                 return DefineShape3Tag.Create(reader);
			case (int)SwfTagType.DefineShape4:                 return DefineShape4Tag.Create(reader);
			case (int)SwfTagType.DefineBitsLossless:           return DefineBitsLosslessTag.Create(reader);
			case (int)SwfTagType.DefineBitsLossless2:          return DefineBitsLossless2Tag.Create(reader);
			case (int)SwfTagType.DefineSprite:                 return DefineSpriteTag.Create(reader);
			case (int)SwfTagType.FileAttributes:               return FileAttributesTag.Create(reader);
			default:                                           return UnknownTag.Create(tag_data.TagId);
			}
		}
	}
}
