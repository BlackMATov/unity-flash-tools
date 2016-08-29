using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimation)), CanEditMultipleObjects]
	public class SwfAnimationEditor : Editor {
		List<SwfAnimation> _animations = new List<SwfAnimation>();

		void AllAnimationsForeachWithUndo(Action<SwfAnimation> act) {
			Undo.RecordObjects(
				_animations.ToArray(),
				"Inspector");
			foreach ( var animation in _animations ) {
				act(animation);
				EditorUtility.SetDirty(animation);
			}
		}

		int GetMinAnimationsFrameCount() {
			return _animations.Count > 0
				? _animations.Min(anim => anim.frameCount)
				: 0;
		}

		string GetAnimationsFrameCountStr() {
			return _animations.Aggregate(string.Empty, (acc, anim) => {
				var frame_count_str = anim.frameCount.ToString();
				return string.IsNullOrEmpty(acc)
					? frame_count_str
					: (acc != frame_count_str ? "--" : acc);
			});
		}

		string GetAnimationsCurrentFrameStr() {
			return _animations.Aggregate(string.Empty, (acc, anim) => {
				var current_frame_str = anim.currentFrame.ToString();
				return string.IsNullOrEmpty(acc)
					? current_frame_str
					: (acc != current_frame_str ? "--" : acc);
			});
		}

		void DrawCurrentFrame() {
			var min_frame_count = GetMinAnimationsFrameCount();
			if ( min_frame_count > 0 ) {
				EditorGUILayout.IntSlider(
					SwfEditorUtils.GetPropertyByName(serializedObject, "_currentFrame"),
					0,
					min_frame_count - 1,
					"Frame");
			}
		}

		void DrawAnimationControls() {
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if ( GUILayout.Button(new GUIContent("<<", "to begin frame")) ) {
					AllAnimationsForeachWithUndo(p => p.ToBeginFrame());
				}
				if ( GUILayout.Button(new GUIContent("<", "to prev frame")) ) {
					AllAnimationsForeachWithUndo(p => p.ToPrevFrame());
				}
				GUILayout.Label(string.Format(
					"{0}/{1}",
					GetAnimationsCurrentFrameStr(), GetAnimationsFrameCountStr()));
				if ( GUILayout.Button(new GUIContent(">", "to next frame")) ) {
					AllAnimationsForeachWithUndo(p => p.ToNextFrame());
				}
				if ( GUILayout.Button(new GUIContent(">>", "to end frame")) ) {
					AllAnimationsForeachWithUndo(p => p.ToEndFrame());
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
			_animations = targets
				.OfType<SwfAnimation>()
				.ToList();
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