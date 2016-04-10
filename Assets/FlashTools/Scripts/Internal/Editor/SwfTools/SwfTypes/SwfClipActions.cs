namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfClipActions {
		public static SwfClipActions identity {
			get {
				return new SwfClipActions();
			}
		}

		public static SwfClipActions Read(SwfStreamReader reader) {
			//TODO: IMPLME
			return SwfClipActions.identity;
		}

		public override string ToString() {
			return "SwfClipActions.";
		}
	}
}