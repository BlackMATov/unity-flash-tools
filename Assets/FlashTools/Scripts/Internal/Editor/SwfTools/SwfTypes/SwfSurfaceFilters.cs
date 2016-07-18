using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfSurfaceFilters {
		public static SwfSurfaceFilters identity {
			get {
				return new SwfSurfaceFilters();
			}
		}

		public static SwfSurfaceFilters Read(SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException("Surface filters is unsupported");
		}

		public override string ToString() {
			return "SwfSurfaceFilters.";
		}
	}
}