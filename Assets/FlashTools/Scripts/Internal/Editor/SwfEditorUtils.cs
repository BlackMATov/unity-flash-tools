using UnityEngine;
using UnityEditor;

using System;
using System.IO;

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

		public static void DeleteAnimationAssetWithDepends(SwfAnimationAsset asset) {
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

		public static string GetSwfPathFromSettingsPath(string settings_path) {
			return Path.ChangeExtension(
				Path.Combine(
					Path.GetDirectoryName(settings_path),
					Path.GetFileNameWithoutExtension(settings_path)),
				".swf");
		}

		public static string GetAtlasPathFromSwfPath(string swf_path) {
			return Path.ChangeExtension(swf_path, ".settings.png");
		}

		public static string GetAtlasPathFromSettingsPath(string settings_path) {
			return Path.ChangeExtension(settings_path, ".png");
		}

		public static string GetSettingsPathFromSwfPath(string swf_path) {
			return Path.ChangeExtension(swf_path, ".settings.asset");
		}
	}
}