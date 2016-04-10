namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfSurfaceFilters {
		public static SwfSurfaceFilters identity {
			get {
				return new SwfSurfaceFilters();
			}
		}

		public static SwfSurfaceFilters Read(SwfStreamReader reader) {
			//TODO: IMPLME
			return SwfSurfaceFilters.identity;
		}

		public override string ToString() {
			return "SwfSurfaceFilters.";
		}
	}
}