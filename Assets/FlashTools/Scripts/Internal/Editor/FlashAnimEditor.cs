using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(FlashAnim))]
	public class FlashAnimEditor : Editor {
		//FlashAnim _anim = null;

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

		void OnEnable() {
			//_anim = target as FlashAnim;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
		}
	}
}