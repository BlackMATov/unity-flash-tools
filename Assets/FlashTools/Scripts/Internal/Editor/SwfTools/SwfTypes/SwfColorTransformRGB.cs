namespace FlashTools.Internal.SwfTools.SwfTypes {
	struct SwfColorTransformRGB {
		public short RMul;
		public short GMul;
		public short BMul;
		public bool  HasMul;
		public short RAdd;
		public short GAdd;
		public short BAdd;
		public bool  HasAdd;

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
				"SwfColorTransformRGB. " +
				"RMul: {0}, GMul: {1}, BMul: {2}, HasMul: {3}, " +
				"RAdd: {4}, GAdd: {5}, BAdd: {6}, HasAdd: {7}",
				RMul, GMul, GMul, HasMul,
				RAdd, GAdd, BAdd, HasAdd);
		}

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
	}
}