using UnityEngine;
using UnityEditor;

using System;

namespace FlashTools.Internal {
	public static class SwfEditorUtils {
		public static void DoWithMixedValue(bool mixed, Action act) {
			var last_show_mixed_value = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = mixed;
			act();
			EditorGUI.showMixedValue = last_show_mixed_value;
		}

		public static void DoWithEnabledGUI(bool enabled, Action act) {
			var last_gui_enabled = GUI.enabled;
			GUI.enabled = enabled;
			act();
			GUI.enabled = last_gui_enabled;
		}

		public static SerializedProperty GetPropertyByName(SerializedObject obj, string name) {
			var prop = obj.FindProperty(name);
			if ( prop == null ) {
				throw new UnityException(string.Format(
					"SwfEditorUtils. Not found property: {0}",
					name));
			}
			return prop;
		}
	}
}