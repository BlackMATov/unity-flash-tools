using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfColorTransform {
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

		public static SwfColorTransform identity {
			get {
				return new SwfColorTransform {
					RMul   = byte.MaxValue,
					GMul   = byte.MaxValue,
					BMul   = byte.MaxValue,
					AMul   = byte.MaxValue,
					HasMul = false,
					RAdd   = 0,
					GAdd   = 0,
					BAdd   = 0,
					AAdd   = 0,
					HasAdd = false};
			}
		}

		public static SwfColorTransform Read(SwfStreamReader reader, bool with_alpha) {
			var transform    = SwfColorTransform.identity;
			transform.HasAdd = reader.ReadBit();
			transform.HasMul = reader.ReadBit();
			var bits         = reader.ReadUnsignedBits(4);
			if ( transform.HasMul ) {
				transform.RMul = (short)reader.ReadSignedBits(bits);
				transform.GMul = (short)reader.ReadSignedBits(bits);
				transform.BMul = (short)reader.ReadSignedBits(bits);
				transform.AMul = with_alpha ? (short)reader.ReadSignedBits(bits) : byte.MaxValue;
			}
			if ( transform.HasAdd ) {
				transform.RAdd = (short)reader.ReadSignedBits(bits);
				transform.GAdd = (short)reader.ReadSignedBits(bits);
				transform.BAdd = (short)reader.ReadSignedBits(bits);
				transform.AAdd = with_alpha ? (short)reader.ReadSignedBits(bits) : (short)0;
			}
			reader.AlignToByte();
			return transform;
		}

		public override string ToString() {
			return string.Format(
				"SwfColorTransform. " +
				"RMul: {0}, GMul: {1}, BMul: {2}, AMul: {3}, HasMul: {4}, " +
				"RAdd: {5}, GAdd: {6}, BAdd: {7}, AAdd: {8}, HasAdd: {9}",
				RMul, GMul, GMul, AMul, HasMul,
				RAdd, GAdd, BAdd, AAdd, HasAdd);
		}

		public SwfAnimationColorTransform ToAnimationColorTransform() {
			var trans = SwfAnimationColorTransform.identity;
			if ( HasAdd ) {
				trans.Add = new Vector4(
					RAdd / 255.0f,
					GAdd / 255.0f,
					BAdd / 255.0f,
					AAdd / 255.0f);
			}
			if ( HasMul ) {
				trans.Mul = new Vector4(
					RMul / 255.0f,
					GMul / 255.0f,
					BMul / 255.0f,
					AMul / 255.0f);
			}
			return trans;
		}
	}
}