using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfBlendMode {
		public enum Mode {
			Normal,
			Layer,
			Multiply,
			Screen,
			Lighten,
			Darken,
			Difference,
			Add,
			Subtract,
			Invert,
			Alpha,
			Erase,
			Overlay,
			Hardlight
		}
		public Mode Value;

		public static SwfBlendMode Read(SwfStreamReader reader) {
			var mode_id = reader.ReadByte();
			var mode    = ModeFromByte(mode_id);
			return new SwfBlendMode{Value = mode};
		}

		public override string ToString() {
			return string.Format(
				"SwfBlendMode. Mode: {0}",
				Value);
		}

		static Mode ModeFromByte(byte mode_id) {
			switch ( mode_id ) {
			case  0:
			case  1: return Mode.Normal;
			case  2: return Mode.Layer;
			case  3: return Mode.Multiply;
			case  4: return Mode.Screen;
			case  5: return Mode.Lighten;
			case  6: return Mode.Darken;
			case  7: return Mode.Difference;
			case  8: return Mode.Add;
			case  9: return Mode.Subtract;
			case 10: return Mode.Invert;
			case 11: return Mode.Alpha;
			case 12: return Mode.Erase;
			case 13: return Mode.Overlay;
			case 14: return Mode.Hardlight;
			default:
				Debug.LogWarningFormat("Incorrect BlendMode Id: {0}", mode_id);
				return Mode.Normal;
			}
		}
	}
}