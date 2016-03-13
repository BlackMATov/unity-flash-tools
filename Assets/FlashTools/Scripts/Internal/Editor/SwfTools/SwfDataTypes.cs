using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FlashTools.Internal.SwfTools.SwfTags;

namespace FlashTools.Internal.SwfTools {
	enum SwfBlendMode : byte {
		Normal = 0,
		Normal1 = 1,
		Layer = 2,
		Multiply = 3,
		Screen  = 4,
		Lighten = 5,
		Darken = 6,
		Difference = 7,
		Add = 8,
		Subtract = 9,
		Invert = 10,
		Alpha = 11,
		Erase = 12,
		Overlay = 13,
		Hardlight = 14
	}

	struct SwfShortHeader {
		public string Format;
		public byte   Version;
		public uint   FileLength;

		public static SwfShortHeader Read(SwfStreamReader reader) {
			var header        = new SwfShortHeader();
			header.Format     = new string(reader.ReadChars(3));
			header.Version    = reader.ReadByte();
			header.FileLength = reader.ReadUInt32();
			return header;
		}

		public void Write(Stream stream) {
			if ( Format == null || Format.Length != 3 ) {
				throw new UnityException("Incorrect SwfShortHeader Format");
			}
			stream.WriteByte((byte)Format[0]);
			stream.WriteByte((byte)Format[1]);
			stream.WriteByte((byte)Format[2]);
			stream.WriteByte(Version);
			stream.WriteByte((byte)((FileLength >>  0) & 0xff));
			stream.WriteByte((byte)((FileLength >>  8) & 0xff));
			stream.WriteByte((byte)((FileLength >> 16) & 0xff));
			stream.WriteByte((byte)((FileLength >> 24) & 0xff));
		}

		public override string ToString() {
			return string.Format(
				"Format: {0}, Version: {1}, FileLength: {2}",
				Format, Version, FileLength);
		}
	}

	struct SwfLongHeader {
		public SwfShortHeader ShortHeader;
		public SwfRect        FrameSize;
		public float          FrameRate;
		public ushort         FrameCount;

		public static SwfLongHeader Read(SwfStreamReader reader) {
			var header         = new SwfLongHeader();
			header.ShortHeader = SwfShortHeader.Read(reader);
			header.FrameSize   = SwfRect.Read(reader);
			header.FrameRate   = reader.ReadFixedPoint8();
			header.FrameCount  = reader.ReadUInt16();
			return header;
		}

		public override string ToString() {
			return string.Format(
				"Format: {0}, Version: {1}, FileLength: {2}, " +
				"FrameSize: {3}, FrameRate: {4}, FrameCount: {5}",
				ShortHeader.Format, ShortHeader.Version, ShortHeader.FileLength,
				FrameSize, FrameRate, FrameCount);
		}
	}

	struct SwfRGB {
		public byte R;
		public byte G;
		public byte B;

		public static SwfRGB Read(SwfStreamReader reader) {
			var rgb = new SwfRGB();
			rgb.R   = reader.ReadByte();
			rgb.G   = reader.ReadByte();
			rgb.B   = reader.ReadByte();
			return rgb;
		}

		public override string ToString() {
			return string.Format(
				"R: {0}, G: {1}, B: {2}",
				R, G, B);
		}
	}

	struct SwfRGBA {
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public static SwfRGBA Read(SwfStreamReader reader) {
			var rgba = new SwfRGBA();
			rgba.R   = reader.ReadByte();
			rgba.G   = reader.ReadByte();
			rgba.B   = reader.ReadByte();
			rgba.A   = reader.ReadByte();
			return rgba;
		}

		public override string ToString() {
			return string.Format(
				"R: {0}, G: {1}, B: {2}, A: {3}",
				R, G, B, A);
		}
	}

	struct SwfShapesWithStyle {
		public static SwfShapesWithStyle Read(SwfStreamReader reader) {
			return new SwfShapesWithStyle();
		}

		public override string ToString() {
			return "";
		}
	}

	struct SwfRect {
		public float XMin;
		public float XMax;
		public float YMin;
		public float YMax;

		public static SwfRect Read(SwfStreamReader reader) {
			var rect  = new SwfRect();
			var bits  = reader.ReadUnsignedBits(5);
			rect.XMin = reader.ReadSignedBits(bits) / 20.0f;
			rect.XMax = reader.ReadSignedBits(bits) / 20.0f;
			rect.YMin = reader.ReadSignedBits(bits) / 20.0f;
			rect.YMax = reader.ReadSignedBits(bits) / 20.0f;
			reader.AlignToByte();
			return rect;
		}

		public override string ToString() {
			return string.Format(
				"XMin: {0}, XMax: {1}, YMin: {2}, YMax: {3}",
				XMin, XMax, YMin, YMax);
		}
	}

	struct SwfMatrix {
		public float ScaleX;
		public float ScaleY;
		public float RotateSkew0;
		public float RotateSkew1;
		public float TranslateX;
		public float TranslateY;

		public static SwfMatrix Identity {
			get {
				return new SwfMatrix {
					ScaleX      = 1,
					ScaleY      = 1,
					RotateSkew0 = 0,
					RotateSkew1 = 0,
					TranslateX  = 0,
					TranslateY  = 0};
			}
		}

		public static SwfMatrix Read(SwfStreamReader reader) {
			var matrix = SwfMatrix.Identity;
			var has_scale = reader.ReadBit();
			if ( has_scale ) {
				var bits      = (byte)reader.ReadUnsignedBits(5);
				matrix.ScaleX = reader.ReadFixedPoint16(bits);
				matrix.ScaleY = reader.ReadFixedPoint16(bits);
			}
			var has_rotate = reader.ReadBit();
			if ( has_rotate ) {
				var bits           = (byte)reader.ReadUnsignedBits(5);
				matrix.RotateSkew0 = reader.ReadFixedPoint16(bits);
				matrix.RotateSkew1 = reader.ReadFixedPoint16(bits);
			}
			var translate_bits = (byte)reader.ReadUnsignedBits(5);
			matrix.TranslateX  = reader.ReadSignedBits(translate_bits) / 20.0f;
			matrix.TranslateY  = reader.ReadSignedBits(translate_bits) / 20.0f;
			reader.AlignToByte();
			return matrix;
		}

		public override string ToString() {
			return string.Format(
				"ScaleX: {0}, ScaleY: {1}, RotateSkew0: {2}, RotateSkew1: {3}, TranslateX: {4}, TranslateY: {5}",
				ScaleX, ScaleY, RotateSkew0, RotateSkew1, TranslateX, TranslateY);
		}
	}

	struct SwfColorTransformRGB {
		public short RMul;
		public short GMul;
		public short BMul;
		public bool  HasMul;
		public short RAdd;
		public short GAdd;
		public short BAdd;
		public bool  HasAdd;

		public static SwfColorTransformRGB Identity {
			get {
				return new SwfColorTransformRGB {
					RMul   = 1,
					GMul   = 1,
					BMul   = 1,
					HasMul = false,
					RAdd   = 0,
					GAdd   = 0,
					BAdd   = 0,
					HasAdd = false};
			}
		}

		public static SwfColorTransformRGB Read(SwfStreamReader reader) {
			var transform = SwfColorTransformRGB.Identity;
			var has_add = reader.ReadBit();
			var has_mul = reader.ReadBit();
			var bits    = reader.ReadUnsignedBits(4);
			if ( has_mul ) {
				transform.RMul   = (short)reader.ReadSignedBits(bits);
				transform.GMul   = (short)reader.ReadSignedBits(bits);
				transform.BMul   = (short)reader.ReadSignedBits(bits);
				transform.HasMul = true;
			}
			if ( has_add ) {
				transform.RAdd   = (short)reader.ReadSignedBits(bits);
				transform.GAdd   = (short)reader.ReadSignedBits(bits);
				transform.BAdd   = (short)reader.ReadSignedBits(bits);
				transform.HasAdd = true;
			}
			reader.AlignToByte();
			return transform;
		}

		public override string ToString() {
			return string.Format(
				"RMul: {0}, GMul: {1}, BMul: {2}, HasMul: {3}, RAdd: {4}, GAdd: {5}, BAdd: {6}, HasAdd: {7}",
				RMul, GMul, GMul, HasMul, RAdd, GAdd, BAdd, HasAdd);
		}
	}

	struct SwfColorTransformRGBA {
		public short RMul;
		public short GMul;
		public short BMul;
		public short AMul;
		public bool  HasMul;
		public short RAdd;
		public short GAdd;
		public short BAdd;
		public short AAdd;
		public bool  HasAdd;

		public static SwfColorTransformRGBA Identity {
			get {
				return new SwfColorTransformRGBA {
					RMul   = 1,
					GMul   = 1,
					BMul   = 1,
					AMul   = 1,
					HasMul = false,
					RAdd   = 0,
					GAdd   = 0,
					BAdd   = 0,
					AAdd   = 0,
					HasAdd = false};
			}
		}

		public static SwfColorTransformRGBA Read(SwfStreamReader reader) {
			var transform = SwfColorTransformRGBA.Identity;
			var has_add = reader.ReadBit();
			var has_mul = reader.ReadBit();
			var bits    = reader.ReadUnsignedBits(4);
			if ( has_mul ) {
				transform.RMul   = (short)reader.ReadSignedBits(bits);
				transform.GMul   = (short)reader.ReadSignedBits(bits);
				transform.BMul   = (short)reader.ReadSignedBits(bits);
				transform.AMul   = (short)reader.ReadSignedBits(bits);
				transform.HasMul = true;
			}
			if ( has_add ) {
				transform.RAdd   = (short)reader.ReadSignedBits(bits);
				transform.GAdd   = (short)reader.ReadSignedBits(bits);
				transform.BAdd   = (short)reader.ReadSignedBits(bits);
				transform.AAdd   = (short)reader.ReadSignedBits(bits);
				transform.HasAdd = true;
			}
			reader.AlignToByte();
			return transform;
		}

		public override string ToString() {
			return string.Format(
				"RMul: {0}, GMul: {1}, BMul: {2}, AMul: {3}, HasMul: {4}, RAdd: {5}, GAdd: {6}, BAdd: {7}, AAdd: {8}, HasAdd: {9}",
				RMul, GMul, GMul, AMul, HasMul, RAdd, GAdd, BAdd, AAdd, HasAdd);
		}
	}

	struct SwfClipActions {
		public static SwfClipActions Read(SwfStreamReader reader) {
			throw new UnityException("implme!");
		}
	}

	struct SwfSurfaceFilters {
		public static SwfSurfaceFilters Read(SwfStreamReader reader) {
			throw new UnityException("implme!");
		}
	}

	struct SwfControlTags {
		public List<SwfTagBase> Tags;
		public static SwfControlTags Read(SwfStreamReader reader) {
			var control_tags = new SwfControlTags();
			control_tags.Tags = new List<SwfTagBase>();
			while ( true ) {
				var tag = SwfTagBase.Read(reader);
				control_tags.Tags.Add(tag);
				if ( tag.TagType == SwfTagType.End ) {
					break;
				}
			}
			return control_tags;
		}
	}
}