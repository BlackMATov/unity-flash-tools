using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimation)), CanEditMultipleObjects]
	public class SwfAnimationEditor : Editor {
		SwfAnimation _animation = null;

		void DrawCurrentFrame() {
			if ( _animation.frameCount > 1 ) {
				var new_current_frame = EditorGUILayout.IntSlider(
					"Frame", _animation.currentFrame,
					0, _animation.frameCount - 1);
				if ( new_current_frame != _animation.currentFrame ) {
					_animation.currentFrame = new_current_frame;
				}
			}
		}

		void DrawAnimationControls() {
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( GUILayout.Button(new GUIContent("<<", "to begin frame")) ) {
					_animation.ToBeginFrame();
				}
				if ( GUILayout.Button(new GUIContent("<", "to prev frame")) ) {
					_animation.ToPrevFrame();
				}
				GUILayout.Label(string.Format(
					"{0}/{1}",
					_animation.currentFrame, _animation.frameCount));
				if ( GUILayout.Button(new GUIContent(">", "to next frame")) ) {
					_animation.ToNextFrame();
				}
				if ( GUILayout.Button(new GUIContent(">>", "to end frame")) ) {
					_animation.ToEndFrame();
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		// ---------------------------------------------------------------------
		//
		// Messages
		//
		// ---------------------------------------------------------------------

		void OnEnable() {
			_animation = target as SwfAnimation;
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawDefaultInspector();
			DrawCurrentFrame();
			DrawAnimationControls();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}