using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfManager))]
	public class SwfManagerEditor : Editor {
		SwfManager _manager = null;

		void DrawAnimationCount() {
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				EditorGUILayout.IntField(
					"Animation count",
					_manager.AllAnimationCount);
			});
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