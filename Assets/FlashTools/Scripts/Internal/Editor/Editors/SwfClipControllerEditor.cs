using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfClipController)), CanEditMultipleObjects]
	public class SwfClipControllerEditor : Editor {
		List<SwfClipController> _controllers = new List<SwfClipController>();

		void AllControllersForeach(System.Action<SwfClipController> act) {
			foreach ( var controller in _controllers ) {
				act(controller);
			}
		}

		void DrawClipControls() {
			SwfEditorUtils.DoRightHorizontalGUI(() => {
				if ( GUILayout.Button("Stop") ) {
					AllControllersForeach(ctrl => ctrl.Stop(ctrl.isStopped));
				}
				if ( GUILayout.Button("Play") ) {
					AllControllersForeach(ctrl => ctrl.Play(ctrl.isPlaying));
				}
			});
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_controllers = targets.OfType<SwfClipController>().ToList();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawDefaultInspector();
			if ( Application.isPlaying ) {
				DrawClipControls();
			}
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}