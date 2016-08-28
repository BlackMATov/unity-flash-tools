using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfManager))]
	public class SwfManagerEditor : Editor {
		SwfManager _manager = null;

		void DrawAnimationCount() {
			var last_gui_enabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.IntField(
				"Animation count",
				_manager.AllAnimationCount);
			GUI.enabled = last_gui_enabled;
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_manager = target as SwfManager;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			DrawAnimationCount();
		}
	}
}