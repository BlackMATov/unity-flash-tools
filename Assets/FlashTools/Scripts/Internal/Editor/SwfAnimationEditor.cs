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
				var frame_count     = anim.frameCount > 0 ? anim.frameCount - 1 : 0;
				var frame_count_str = frame_count.ToString();
				return string.IsNullOrEmpty(acc)
					? frame_count_str
					: (acc != frame_count_str ? "--" : acc);
			});
		}

		string GetAnimationsCurrentFrameStr() {
			return _animations.Aggregate(string.Empty, (acc, anim) => {
				var current_frame     = anim.currentFrame;
				var current_frame_str = current_frame.ToString();
				return string.IsNullOrEmpty(acc)
					? current_frame_str
					: (acc != current_frame_str ? "--" : acc);
			});
		}

		bool IsAllAnimationsHasOneClip() {
			foreach ( var animation in _animations ) {
				if ( !animation.clip ) {
					return false;
				}
				if ( animation.clip != _animations.First().clip ) {
					return false;
				}
			}
			return true;
		}

		List<string> GetAllSequences(bool include_empty) {
			var seq_set = new HashSet<string>(_animations
				.Where(p => p.clip)
				.SelectMany(p => p.clip.Sequences)
				.Select(p => p.Name));
			if ( include_empty ) {
				seq_set.Add(string.Empty);
			}
			return seq_set.ToList();
		}

		void DrawSequence() {
			if ( IsAllAnimationsHasOneClip() ) {
				var sequence_prop = SwfEditorUtils.GetPropertyByName(serializedObject, "_sequence");
				SwfEditorUtils.DoWithMixedValue(
					sequence_prop.hasMultipleDifferentValues, () => {
						var all_sequences  = GetAllSequences(true);
						var sequence_index = EditorGUILayout.Popup(
							"Sequence",
							sequence_prop.hasMultipleDifferentValues
								? all_sequences.FindIndex(p => string.IsNullOrEmpty(p))
								: all_sequences.FindIndex(p => p == sequence_prop.stringValue),
							all_sequences.ToArray());
						var new_sequence = all_sequences[sequence_index];
						if ( !string.IsNullOrEmpty(new_sequence) ) {
							if ( sequence_prop.hasMultipleDifferentValues ) {
								sequence_prop.stringValue = string.Empty;
							}
							sequence_prop.stringValue = new_sequence;
							sequence_prop.serializedObject.ApplyModifiedProperties();
						}
					});
			}
		}

		void DrawCurrentFrame() {
			var min_frame_count = GetMinAnimationsFrameCount();
			if ( min_frame_count > 1 ) {
				EditorGUILayout.IntSlider(
					SwfEditorUtils.GetPropertyByName(serializedObject, "_currentFrame"),
					0,
					min_frame_count - 1,
					"Current frame");
				DrawAnimationControls();
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
			DrawSequence();
			DrawCurrentFrame();
			if ( GUI.changed ) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}