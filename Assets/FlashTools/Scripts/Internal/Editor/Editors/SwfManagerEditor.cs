using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfManager))]
	public class SwfManagerEditor : Editor {
		SwfManager _manager = null;

		void DrawCounts() {
			SwfEditorUtils.DoWithEnabledGUI(false, () => {
				EditorGUILayout.IntField(
					"Clip count",
					_manager.clipCount);
				EditorGUILayout.IntField(
					"Controller count",
					_manager.controllerCount);
			});
		}

		void DrawControls() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( _manager.isPaused && GUILayout.Button("Resume") ) {
					_manager.Resume();
				}
				if ( _manager.isPlaying && GUILayout.Button("Pause") ) {
					_manager.Pause();
				}
			}
			GUILayout.EndHorizontal();
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
			serializedObject.Update();
			DrawDefaultInspector();
			DrawCounts();
			if ( Application.isPlaying ) {
				DrawControls();
			}
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}