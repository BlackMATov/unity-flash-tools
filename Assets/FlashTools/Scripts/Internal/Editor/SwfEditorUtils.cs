using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

namespace FlashTools.Internal {
	public static class SwfEditorUtils {
		public static void DoWithMixedValue(bool mixed, System.Action act) {
			var last_show_mixed_value = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = mixed;
			try {
				act();
			} finally {
				EditorGUI.showMixedValue = last_show_mixed_value;
			}
		}

		public static void DoWithEnabledGUI(bool enabled, System.Action act) {
			var last_gui_enabled = GUI.enabled;
			GUI.enabled = enabled;
			try {
				act();
			} finally {
				GUI.enabled = last_gui_enabled;
			}
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

		public static void DeleteAssetWithDepends(SwfAsset asset) {
			if ( asset ) {
				if ( asset.Atlas ) {
					AssetDatabase.DeleteAsset(
						AssetDatabase.GetAssetPath(asset.Atlas));
				}
				AssetDatabase.DeleteAsset(
					AssetDatabase.GetAssetPath(asset));
			}
		}

		public static void RemoveAllSubAssets(string asset_path) {
			var assets = AssetDatabase.LoadAllAssetsAtPath(asset_path);
			foreach ( var asset in assets ) {
				if ( !AssetDatabase.IsMainAsset(asset) ) {
					GameObject.DestroyImmediate(asset, true);
				}
			}
		}

		public static T LoadOrCreateAsset<T>(string asset_path) where T : ScriptableObject {
			var asset = AssetDatabase.LoadAssetAtPath<T>(asset_path);
			if ( !asset ) {
				asset = ScriptableObject.CreateInstance<T>();
				AssetDatabase.CreateAsset(asset, asset_path);
			}
			return asset;
		}

		public static string GetAtlasPathFromAsset(SwfAsset asset) {
			var asset_path = AssetDatabase.GetAssetPath(asset);
			return Path.ChangeExtension(asset_path, "._Atlas_.png");
		}
	}
}