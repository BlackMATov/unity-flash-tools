using UnityEngine;
using UnityEditor;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimation)), CanEditMultipleObjects]
	public class SwfAnimationEditor : Editor {
		SwfAnimation _animation = null;

		SerializedProperty GetCurrentFrameProperty() {
			var prop = serializedObject.FindProperty("_currentFrame");
			if ( prop == null ) {
				throw new UnityException("SwfAnimationEditor. Not found current frame property");
			}
			return prop;
		}

		void DrawCurrentFrame() {
			if ( _animation.frameCount > 1 ) {
				Undo.RecordObject(_animation, "Change SwfAnimation frame");
				EditorGUILayout.IntSlider(
					GetCurrentFrameProperty(),
					0,
					_animation.frameCount - 1,
					"Frame");
			}
		}

		void DrawAnimationControls() {
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( GUILayout.Button(new GUIContent("<<", "to begin frame")) ) {
					Undo.RecordObject(_animation, "Change SwfAnimation frame");
					_animation.ToBeginFrame();
					EditorUtility.SetDirty(_animation);
				}
				if ( GUILayout.Button(new GUIContent("<", "to prev frame")) ) {
					Undo.RecordObject(_animation, "Change SwfAnimation frame");
					_animation.ToPrevFrame();
					EditorUtility.SetDirty(_animation);
				}
				GUILayout.Label(string.Format(
					"{0}/{1}",
					_animation.currentFrame, _animation.frameCount));
				if ( GUILayout.Button(new GUIContent(">", "to next frame")) ) {
					Undo.RecordObject(_animation, "Change SwfAnimation frame");
					_animation.ToNextFrame();
					EditorUtility.SetDirty(_animation);
				}
				if ( GUILayout.Button(new GUIContent(">>", "to end frame")) ) {
					Undo.RecordObject(_animation, "Change SwfAnimation frame");
					_animation.ToEndFrame();
					EditorUtility.SetDirty(_animation);
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