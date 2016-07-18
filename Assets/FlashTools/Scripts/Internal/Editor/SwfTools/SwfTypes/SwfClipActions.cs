using UnityEngine;

namespace FlashTools.Internal.SwfTools.SwfTypes {
	public struct SwfClipActions {
		public static SwfClipActions identity {
			get {
				return new SwfClipActions();
			}
		}

		public static SwfClipActions Read(SwfStreamReader reader) {
			//TODO: IMPLME
			throw new UnityException("Clip actions is unsupported");
		}

		public override string ToString() {
			return "SwfClipActions.";
		}
	}
}