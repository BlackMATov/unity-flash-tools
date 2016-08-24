using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FlashTools.Internal {
	[CustomEditor(typeof(SwfAnimation))]
	public class SwfAnimationEditor : Editor {
		SwfAnimation _animation = null;

		void OnEnable() {
			_animation = target as SwfAnimation;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if ( _animation.Asset && _animation.frameCount > 1 ) {
				var new_current_frame = EditorGUILayout.IntSlider(
					"Frame", _animation.currentFrame,
					0, _animation.frameCount - 1);
				if ( new_current_frame != _animation.currentFrame ) {
					_animation.currentFrame = new_current_frame;
				}
			}
		}
	}
}