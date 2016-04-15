using UnityEngine;

namespace FlashTools {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SwfAnimation : MonoBehaviour {
		public SwfAnimationAsset Asset = null;

		int _current_frame = 0;

		public int frameCount {
			get { return Asset ? Asset.Data.Frames.Count : 0; }
		}

		public int currentFrame {
			get { return _current_frame; }
			set { _current_frame = Mathf.Clamp(value, 0, frameCount - 1); }
		}

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------
	}
}