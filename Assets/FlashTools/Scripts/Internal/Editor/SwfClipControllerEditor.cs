using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfClipController)), CanEditMultipleObjects]
	public class SwfClipControllerEditor : Editor {
		List<SwfClipController> _controllers = new List<SwfClipController>();

		void AllControllersForeach(Action<SwfClipController> act) {
			foreach ( var controller in _controllers ) {
				act(controller);
			}
		}

		void DrawClipControls() {
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
				.OfType<SwfClipController>()
				.ToList();
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