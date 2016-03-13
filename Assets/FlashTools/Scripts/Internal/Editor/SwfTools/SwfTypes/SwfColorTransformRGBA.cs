namespace FlashTools.Internal.SwfTools.SwfTypes {
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
				"SwfColorTransformRGBA. " +
				"RMul: {0}, GMul: {1}, BMul: {2}, AMul: {3}, HasMul: {4}, " +
				"RAdd: {5}, GAdd: {6}, BAdd: {7}, AAdd: {8}, HasAdd: {9}",
				RMul, GMul, GMul, AMul, HasMul,
				RAdd, GAdd, BAdd, AAdd, HasAdd);
		}

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
	}
}