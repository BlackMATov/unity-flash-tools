using UnityEngine;
using UnityEditor;
using System.Linq;

namespace FlashTools.Internal {
	[CustomEditor(typeof(FlashAnim))]
	public class FlashAnimEditor : Editor {
		FlashAnim _anim = null;

		// ------------------------------------------------------------------------
		//
		// Messages
		//
		// ------------------------------------------------------------------------

		void OnEnable() {
			_anim = target as FlashAnim;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			if ( _anim.Asset ) {
				var new_current_frame = EditorGUILayout.IntSlider(
					"Frame",
					_anim.currentFrame, 0, _anim.frameCount - 1);
				if ( new_current_frame != _anim.currentFrame ) {
					_anim.currentFrame = new_current_frame;
				}

				var symbols = _anim.Asset.Data.Library.Symbols.Select(p => p.Id).ToArray();
				var new_current_symbol = EditorGUILayout.Popup(
					"Symbol",
					_anim.currentSymbol, symbols);
				if ( new_current_symbol != _anim.currentSymbol ) {
					_anim.currentSymbol = new_current_symbol;
				}
			}
		}
	}
}