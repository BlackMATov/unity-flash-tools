using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimationController)), CanEditMultipleObjects]
	public class SwfAnimationControllerEditor : Editor {
		List<SwfAnimationController> _controllers = new List<SwfAnimationController>();

		void AllControllersForeach(Action<SwfAnimationController> act) {
			foreach ( var controller in _controllers ) {
				act(controller);
			}
		}

		void DrawAnimationControls() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( GUILayout.Button("Stop") ) {
					AllControllersForeach(p => p.Stop());
				}
				if ( GUILayout.Button("Pause") ) {
					AllControllersForeach(p => p.Pause());
				}
				if ( GUILayout.Button("Resume") ) {
					AllControllersForeach(p => p.Resume());
				}
				if ( GUILayout.Button("Play") ) {
					AllControllersForeach(p => p.Play());
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
			_controllers = targets
				.OfType<SwfAnimationController>()
				.ToList();
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawDefaultInspector();
			if ( Application.isPlaying ) {
				DrawAnimationControls();
			}
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}