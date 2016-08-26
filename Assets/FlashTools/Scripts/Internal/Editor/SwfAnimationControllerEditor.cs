using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationController)), CanEditMultipleObjects]
	public class SwfAnimationControllerEditor : Editor {
		SwfAnimationController _controller = null;

		void DrawAnimationControls() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( GUILayout.Button("Stop") ) {
					_controller.Stop();
				}
				if ( GUILayout.Button("Pause") ) {
					_controller.Pause();
				}
				if ( GUILayout.Button("Resume") ) {
					_controller.Resume();
				}
				if ( GUILayout.Button("Play") ) {
					_controller.Play();
				}
			}
			GUILayout.EndHorizontal();
		}

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

		void OnEnable() {
			_controller = target as SwfAnimationController;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			if ( Application.isPlaying ) {
				DrawAnimationControls();
			}
		}
	}
}